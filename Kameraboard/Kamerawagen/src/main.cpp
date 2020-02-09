
#include <Arduino.h>
#include "esp_camera.h"
#include <WiFi.h>
#include "esp_timer.h"
#include "img_converters.h"
#include "fb_gfx.h"
#include "soc/soc.h"          //disable brownout problems
#include "soc/rtc_cntl_reg.h" //disable brownout problems
#include "dl_lib_matrix3d.h"
#include "esp_http_server.h"
#include "EEPROM.h"
#include "ticker.h"

//#define _debug

const uint8_t numChars = 32;
// EEPROM-Adressen
#define setup_done 0x47
// EEPROM-Belegung
// EEPROM-Speicherplätze der Local-IDs
const uint16_t adr_setup_done = 0x00;
const uint16_t adr_ssid = 0x01;
const uint16_t adr_password = adr_ssid + numChars;
const uint16_t adr_round = adr_password + numChars;
const uint16_t adr_format = adr_round + 1;
const uint16_t adr_IP0 = adr_format + 1;
const uint16_t lastAdr = adr_IP0 + 4;
const uint16_t EEPROM_SIZE = lastAdr;

gpio_num_t BUILTIN_LED;
const uint8_t CAN_FRAME_SIZE = 13; /* maximum datagram size */

uint8_t delta_psram = 0;
String formatArray[13] = {
    "QQVGA - 160x120",
    "QQVGA2 - 128x160",
    "QCIF - 176x144",
    "HQVGA - 240x176",
    "QVGA - 320x240",
    "CIF - 400x296",
    "VGA - 640x480",
    "SVGA - 800x600",
    "XGA - 1024x768",
    "SXGA - 1280x1024",
    "UXGA - 1600x1200",
    "QXGA - 2048*1536",
    "INVALID"};

Ticker tckr;
const float tckrTime = 0.01;

enum blinkStatus
{
  blinkFast = 0, // wartet auf Password
  blinkSlow,     // wartet auf WiFi
  blinkNo        // mit WiFi verbunden
};
blinkStatus blink;

// die Portadressen; 15730 und 15731 sind von Märklin festgelegt
// OUT is even
const unsigned int localPortDelta = 4;                                // local port to listen on
const unsigned int localPortoutWDP = 15730;                           // local port to send on
const unsigned int localPortinWDP = 15731;                            // local port to listen on
const unsigned int localPortoutGW = localPortoutWDP + localPortDelta; // local port to send on
const unsigned int localPortinGW = localPortinWDP + localPortDelta;   // local port to listen on

WiFiUDP UdpOUTGW;
WiFiUDP UdpINGW;

#define PART_BOUNDARY "123456789000000000000987654321"

static const char *_STREAM_CONTENT_TYPE = "multipart/x-mixed-replace;boundary=" PART_BOUNDARY;
static const char *_STREAM_BOUNDARY = "\r\n--" PART_BOUNDARY "\r\n";
static const char *_STREAM_PART = "Content-Type: image/jpeg\r\nContent-Length: %u\r\n\r\n";

httpd_handle_t stream_httpd = NULL;
httpd_handle_t camera_httpd = NULL;

String liesEingabe()
{
  boolean newData = false;
  char receivedChars[numChars]; // das Array für die empfangenen Daten
  static byte ndx = 0;
  char endMarker = '\r';
  char rc;

  while (newData == false)
  {
    while (Serial.available() > 0)
    {
      rc = Serial.read();

      if (rc != endMarker)
      {
        receivedChars[ndx] = rc;
        Serial.print(rc);
        ndx++;
        if (ndx >= numChars)
        {
          ndx = numChars - 1;
        }
      }
      else
      {
        receivedChars[ndx] = '\0'; // Beendet den String
        Serial.println();
        ndx = 0;
        newData = true;
      }
    }
  }
  return receivedChars;
}

String netzwerkScan()
{
  // Zunächst Station Mode und Trennung von einem AccessPoint, falls dort eine Verbindung bestand
  WiFi.mode(WIFI_STA);
  WiFi.disconnect();
  delay(100);

  Serial.println("Scan-Vorgang gestartet");

  // WiFi.scanNetworks will return the number of networks found
  int n = WiFi.scanNetworks();
  Serial.println("Scan-Vorgang beendet");
  if (n == 0)
  {
    Serial.println("Keine Netzwerke gefunden!");
  }
  else
  {
    Serial.print(n);
    Serial.println(" Netzwerke gefunden");
    for (int i = 0; i < n; ++i)
    {
      // Drucke SSID and RSSI für jedes gefundene Netzwerk
      Serial.print(i + 1);
      Serial.print(": ");
      Serial.print(WiFi.SSID(i));
      Serial.print(" (");
      Serial.print(WiFi.RSSI(i));
      Serial.print(")");
      Serial.println((WiFi.encryptionType(i) == WIFI_AUTH_OPEN) ? " " : "*");
      delay(10);
    }
  }
  uint8_t number;
  do
  {
    Serial.println("Bitte Netzwerk auswaehlen: ");
    String no = liesEingabe();
    number = uint8_t(no[0]) - uint8_t('0');
  } while ((number > n) || (number == 0));

  return WiFi.SSID(number - 1);
}

// mit dieser Prozedur wird der Videostream aufgenommen
// und weitergeleitet
esp_err_t stream_handler(httpd_req_t *req)
{
  camera_fb_t *fb = NULL;
  esp_err_t res = ESP_OK;
  size_t _jpg_buf_len;
  uint8_t *_jpg_buf;
  char *part_buf[64];
//
#ifdef _debug
  static int64_t last_frame = 0;
  if (!last_frame)
  {
    last_frame = esp_timer_get_time();
  }
#endif
  //
  res = httpd_resp_set_type(req, _STREAM_CONTENT_TYPE);
  if (res != ESP_OK)
    return res;
  while (true)
  {
    fb = esp_camera_fb_get();
    if (!fb)
    {
      ESP_LOGE(TAG, "Kamera-Fehler");
      res = ESP_FAIL;
    }
    else
    {
      _jpg_buf_len = fb->len;
      _jpg_buf = fb->buf;
    }
    if (res == ESP_OK)
    {
      size_t hlen = snprintf((char *)part_buf, 64, _STREAM_PART, _jpg_buf_len);
      res = httpd_resp_send_chunk(req, (const char *)part_buf, hlen);
    }
    if (res == ESP_OK)
      res = httpd_resp_send_chunk(req, (const char *)_jpg_buf, _jpg_buf_len);
    if (res == ESP_OK)
      res = httpd_resp_send_chunk(req, _STREAM_BOUNDARY, strlen(_STREAM_BOUNDARY));
    esp_camera_fb_return(fb);
    if (res != ESP_OK)
      break;
//
#ifdef _debug
    int64_t fr_end = esp_timer_get_time();
    int64_t frame_time = fr_end - last_frame;
    last_frame = fr_end;
    frame_time /= 1000;
    Serial.printf("\r\nMJPG: %uKB %ums (%.1ffps)",
                  (uint32_t)(_jpg_buf_len / 1024),
                  (uint32_t)frame_time, 1000.0 / (uint32_t)frame_time);
#endif
    //
  }
//
#ifdef _debug
  last_frame = 0;
#endif
  //
  return res;
}

void initCamera()
{
  camera_config_t config;
  config.ledc_channel = LEDC_CHANNEL_0;
  config.ledc_timer = LEDC_TIMER_0;
  config.pin_d1 = GPIO_NUM_35;
  config.pin_d2 = GPIO_NUM_34;
  config.pin_d3 = GPIO_NUM_5;
  config.pin_d4 = GPIO_NUM_39;
  config.pin_d5 = GPIO_NUM_18;
  config.pin_d6 = GPIO_NUM_36;
  config.pin_d7 = GPIO_NUM_19;
  config.pin_xclk = GPIO_NUM_27;
  config.pin_pclk = GPIO_NUM_21;
  config.pin_href = GPIO_NUM_26;
  config.pin_sscb_scl = GPIO_NUM_23;
  config.pin_pwdn = -1;
  config.pin_reset = GPIO_NUM_15;
  config.xclk_freq_hz = 20000000;
  config.pixel_format = PIXFORMAT_JPEG;

  //init with high specs to pre-allocate larger buffers
  if (psramFound())
  {
    Serial.println("PSRAM gefunden!");
    config.pin_d0 = GPIO_NUM_32;
    config.pin_vsync = GPIO_NUM_25;
    config.pin_sscb_sda = GPIO_NUM_22;
    config.frame_size = FRAMESIZE_UXGA;
    config.fb_count = 2;
    config.jpeg_quality = 12;
  }
  else
  {
    Serial.println("Kein PSRAM gefunden!");
    config.pin_d0 = GPIO_NUM_17;
    config.pin_vsync = GPIO_NUM_22;
    config.pin_sscb_sda = GPIO_NUM_25;
    config.frame_size = FRAMESIZE_SVGA;
    config.fb_count = 1;
    config.jpeg_quality = 12;
  }
  // Camera init
  esp_err_t err = esp_camera_init(&config);
  if (err != ESP_OK)
  {
    Serial.printf("Kamera-Initialisierung schlug fehl mit Fehler 0x%x\r\n", err);
    return;
  }
}

// die Videodaten werden per http auf den PC übertragen. Hiermit
// wird der zugehörige Server gestartet
void initSensor(framesize_t f)
{
  sensor_t *s = esp_camera_sensor_get();
  s->set_vflip(s, true);
  s->set_hmirror(s, true);
  s->set_quality(s, 12);
  s->set_framesize(s, f);
}

void startCameraServer()
{
  httpd_config_t config = HTTPD_DEFAULT_CONFIG();
  httpd_uri_t stream_uri = {
      .uri = "/stream",
      .method = HTTP_GET,
      .handler = stream_handler,
      .user_ctx = NULL};

  config.server_port = 81;

  config.ctrl_port = config.server_port;
  Serial.printf("Starte den Streamserver am Port: '%d'", config.server_port);
  Serial.println();
  if (httpd_start(&stream_httpd, &config) == ESP_OK)
  {
    httpd_register_uri_handler(stream_httpd, &stream_uri);
  }
}

// die Routin antwortet auf die Anfrage des CANguru-Servers mit CMD 0x88;
// damit erhält er die IP-Adresse der CANguru-Bridge und kann dann
// damit eine Verbindung aufbauen
void proc_IP2GW()
{
  byte UDPbuffer[CAN_FRAME_SIZE]; //buffer to hold incoming packet,
  int packetSize = UdpINGW.parsePacket();
  // if there's data available, read a packet
  if (packetSize)
  {
    // read the packet into packetBuffer
    UdpINGW.read(UDPbuffer, CAN_FRAME_SIZE);
    // send received data via ETHERNET
    IPAddress IPPC = UdpINGW.remoteIP();
    framesize_t format = (framesize_t)UDPbuffer[0x05];
    switch (UDPbuffer[0x01])
    {
    case 0x88:
      switch (UDPbuffer[0x5])
      {
      case 0xFF:
        Serial.print("Habe Kontakt mit IP-Adresse: ");
        Serial.println(IPPC);
        // IP-Adresse des PC für den Neustart merken
        for (uint8_t i = 0; i < 4; i++)
        {
          EEPROM.writeByte(adr_IP0 + i, IPPC[i]);
          EEPROM.commit();
        }
        format = (framesize_t)EEPROM.readByte(adr_format);
        break;
      case FRAMESIZE_CIF:
      case FRAMESIZE_VGA:
      case FRAMESIZE_SVGA:
      case FRAMESIZE_XGA:
      case FRAMESIZE_SXGA:
        // format zurücksetzen
        EEPROM.writeByte(adr_format, UDPbuffer[0x05]);
        EEPROM.commit();
        initSensor((framesize_t)UDPbuffer[0x05]);
        break;
      }
      break;
    }
    Serial.print("Neues Format ");
    Serial.println(formatArray[format]);
    UDPbuffer[0x1]++;
    UDPbuffer[0x04] = 0x02;        // Datenlänge
    UDPbuffer[0x05] = format;      // genutztes Format
    UDPbuffer[0x06] = delta_psram; // format 3 für tooltip
    UdpOUTGW.beginPacket(IPPC, localPortoutGW);
    UdpOUTGW.write(UDPbuffer, CAN_FRAME_SIZE);
    UdpOUTGW.endPacket();
  }
}

// der Timer steuert das Scannen der Slaves, das Blinken der LED
// sowie das Absenden des PING
void timer1s()
{
  static uint8_t secs = 0;
  static uint8_t slices = 0;
  slices++;
  switch (blink)
  {
  case blinkFast:
    if (slices >= 10)
    {
      slices = 0;
      secs++;
    }
    break;
  case blinkSlow:
    if (slices >= 40)
    {
      slices = 0;
      secs++;
    }
    break;
  case blinkNo:
    if (psramFound())
      secs = 2;
    else
      secs = 1;
    break;
  }
  if (secs % 2 == 0)
    // turn the LED on by making the voltage HIGH
    digitalWrite(BUILTIN_LED, HIGH);
  else
    // turn the LED off by making the voltage LOW
    digitalWrite(BUILTIN_LED, LOW);
}

void setup()
{
  WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0); //disable brownout detector

  Serial.begin(115200);
  Serial.setDebugOutput(false);
  Serial.println("\r\n\rKamerawagen plus Version DiMo");

  if (!EEPROM.begin(EEPROM_SIZE))
  {
    Serial.println("EEPROM-Fehler");
  }
  // initialize LED digital pin as an output.
  if (psramFound())
    BUILTIN_LED = GPIO_NUM_14;
  else
    BUILTIN_LED = GPIO_NUM_16;
  pinMode(BUILTIN_LED, OUTPUT);
  tckr.attach(tckrTime, timer1s); // each sec
  String ssid = "";
  String password = "";
  framesize_t format;
  IPAddress IPPC;
  uint8_t round;
  uint8_t setup_todo = EEPROM.read(adr_setup_done);
  if (setup_todo != setup_done)
  {
    // alles fürs erste Mal
    //
    // wurde das Setup bereits einmal durchgeführt?
    // dann wird dieser Anteil übersprungen
    // 47, weil das EEPROM (hoffentlich) nie ursprünglich diesen Inhalt hatte
    blink = blinkFast;
    // liest die ssid ein
    ssid = netzwerkScan();
    EEPROM.writeString(adr_ssid, ssid);
    EEPROM.commit();
    Serial.println();
    // liest das password ein
    Serial.print("Bitte das Passwort eingeben: ");
    password = liesEingabe();
    EEPROM.writeString(adr_password, password);
    EEPROM.commit();
    Serial.println();
    // round zurücksetzen
    round = 0;
    EEPROM.writeByte(adr_round, round);
    EEPROM.commit();
    // format zurücksetzen
    format = FRAMESIZE_SVGA;
    EEPROM.writeByte(adr_format, format);
    EEPROM.commit();
    // IP-Adresse des PCs
    IPPC = {0x00, 0x00, 0x00, 0x00};
    for (uint8_t i = 0; i < 4; i++)
    {
      EEPROM.writeByte(adr_IP0 + i, 0x00);
      EEPROM.commit();
    }
    // setup_done auf "TRUE" setzen
    EEPROM.write(adr_setup_done, setup_done);
    EEPROM.commit();
  }
  else
  {
    blink = blinkSlow;
    ssid = EEPROM.readString(adr_ssid);
    password = EEPROM.readString(adr_password);
    round = EEPROM.readByte(adr_round);
    format = (framesize_t)EEPROM.readByte(adr_format);
    for (uint8_t i = 0; i < 4; i++)
    {
      IPPC[i] = EEPROM.readByte(adr_IP0 + i);
    }
  }
  // Am Wert der Variablen delta_psramX erkennt der
  // Player auf dem PC, ob er es mit einem Modul mit PSRA
  // zu tun hat. In diesem Fall liegen die Format um den Wert
  // delta_psramX höher (siehe hierzu die Aufzählung framesize_t bzw.
  // formatArray).
  // Im Fall KEIN PSRAM nutzt das Programme die Formate
  // CIF, VGA und SVGA.
  // Im Fall MIT PSRAM nutzt das Programme die Formate
  // SVGA, XGA und SXGA (eben zwei Werte höher).
  if (psramFound())
    delta_psram = 2;
  else
    delta_psram = 0;
  initCamera();
  initSensor(format);

  char ssidCh[ssid.length() + 1];
  ssid.toCharArray(ssidCh, ssid.length() + 1);
  char passwordCh[password.length() + 1];
  password.toCharArray(passwordCh, password.length() + 1);

  // Connect to Wi-Fi network with SSID and password
  Serial.print("Verbinde mit dem Netzwerk -");
  Serial.print(ssidCh);
  Serial.println("-");
  WiFi.begin(ssidCh, passwordCh);
  uint8_t trials = 0;
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
    trials++;
    if (trials > 6)
    {
      // zuviele Versuche für diese Runde
      trials = 0;
      round++;
      if (round > 3)
      // zuviele Runden
      {
        // alles neu laden
        Serial.print("X");
        EEPROM.writeByte(adr_setup_done, 0x00);
        EEPROM.commit();
      }
      Serial.print(round);
      EEPROM.writeByte(adr_round, round);
      EEPROM.commit();
      // ... und Neustart
      ESP.restart();
    }
  }
  // WLAN hat funktioniert
  blink = blinkNo;
  // round zurücksetzen
  round = 0;
  EEPROM.writeByte(adr_round, round);
  EEPROM.commit();
  // Print local IP address and start web server
  Serial.println();
  Serial.print("Eigene IP-Adresse: ");
  Serial.println(WiFi.localIP());
  if (UdpINGW.begin(localPortinGW) == 0)
    Serial.println("Fehler Port Eingang");
  else
    Serial.println("Port Eingang erfolgreich verbunden");
  if (UdpOUTGW.begin(localPortoutGW) == 0)
    Serial.println("Fehler Port Ausgang");
  else
    Serial.println("Port Ausgang erfolgreich verbunden");
  // starte den Kamera-Server
  startCameraServer();
  uint8_t M_PATTERN[] = {0x00, 0x89, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
  M_PATTERN[0X05] = format;
  if (IPPC[0x00] != 0x00)
  {
    Serial.print("Neues Format ");
    Serial.println(formatArray[format]);
    M_PATTERN[0x06] = delta_psram;
    UdpOUTGW.beginPacket(IPPC, localPortoutGW);
    UdpOUTGW.write(M_PATTERN, CAN_FRAME_SIZE);
    UdpOUTGW.endPacket();
  }
  tckr.detach();
}

void loop()
{
  proc_IP2GW();
}