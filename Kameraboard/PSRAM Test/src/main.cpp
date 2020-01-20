
#include <Arduino.h>
extern "C"
{
#include <esp_spiram.h>
#include <esp_himem.h>
}
#include <stdio.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "esp_system.h"
#include "esp_spi_flash.h"

//#define LED GPIO_NUM_16
gpio_num_t BLINKING_LED = GPIO_NUM_16;
bool PSRAM = false;

void setup()
{
  Serial.begin(115200);
  Serial.setDebugOutput(false);
    printf("\nHallo Welt!\n\n");

  // Print chip information
  esp_chip_info_t chip_info;
  esp_chip_info(&chip_info);
  printf("Dies ist ein ESP32-Chip mit %d CPU Kernen,\nWiFi%s%s\n",
         chip_info.cores,
         (chip_info.features & CHIP_FEATURE_BT) ? "/BT" : "",
         (chip_info.features & CHIP_FEATURE_BLE) ? "/BLE" : "");

  printf("Silicon Revision %d\n", chip_info.revision);

  printf("%dMB %s flash\n", spi_flash_get_chip_size() / (1024 * 1024),
         (chip_info.features & CHIP_FEATURE_EMB_FLASH) ? "embedded" : "external");
  vTaskDelay(1000 / portTICK_PERIOD_MS);

  esp_spiram_init_cache();
  if (esp_spiram_get_chip_size() == ESP_SPIRAM_SIZE_INVALID)
  {
    printf("KEIN PSRAM\n");
    BLINKING_LED = GPIO_NUM_16;
    PSRAM = false;
  }
  else if (!esp_spiram_test())
  {
    printf("PSRAM Fehler\n");
  }
  else
  {
    printf("PSRAM OK\n");
    BLINKING_LED = GPIO_NUM_14;
    PSRAM = true;
  }
  printf("Test Ende.\n");
  // initialize LED digital pin as an output.
  pinMode(BLINKING_LED, OUTPUT);
  // turn the LED off by making the voltage LOW
  digitalWrite(BLINKING_LED, HIGH);

  bool onoff = true;
  while (true) 
  {
    if (PSRAM)
      delay(500);
    else
      delay(100);
    Serial.print(".");
    if (onoff)
      // turn the LED on by making the voltage HIGH
      digitalWrite(BLINKING_LED, HIGH);
    else
      // turn the LED on by making the voltage HIGH
      digitalWrite(BLINKING_LED, LOW);
    onoff = !onoff;
  }
}

void loop()
{
  delay(1);
}
