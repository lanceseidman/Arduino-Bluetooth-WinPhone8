#include <SoftwareSerial.h>

int BTTx = 2; // Sparkfun Bluetooth Module: TX
int BTRx = 3; // Sparkfun Bluetooth Module: RX
int led = 13; // My LED to Lightup

// Setup SoftwareSerial to use 'BT' & Read TX/RX Pins
SoftwareSerial BT(BTTx, BTRx); 

void setup()
{
  // Setup LED
  pinMode(led, OUTPUT);
  
  // Setup USB Serial2PC (Listen/Output Received in Serial Monitor)
  Serial.begin(9600);

  // Setup Bluetooth for WP8
  BT.begin(115200);
  // Tell BT to Enter Command Mode (IMPORTANT!!!)
  BT.print("$$$");
  // Take a break, now Attempt (Too quick can cause failure)
  delay(100);
  // Set to 9600 Baud; 115200 too Fast (NewSoftSerial)
  // 9600 Buad change recommended on Arduino Website
  BT.println("U,9600,N");
  // Setup Bluetooth to 9600 & Listen Again
  BT.begin(9600);
}

void loop()
{
  // Setup LED ON (Since I have the Motor on first)
  digitalWrite(led, HIGH);

  // Read BT Module IF it Exists/Available
  if(BT.available())
  {
    // Any incoming Data will be Stored to GetBT
    char GetBT = (char)BT.read();
    // Print out what we Received (Testing)
    Serial.print(GetBT);
    BT.print("Hello");
    // In Windows Phone we made a incoming of 3
    if(GetBT == '3')
    {
      
      
      // Set LED Off
      digitalWrite(led, LOW);
      delay(5000); // Pause for 5s
      
      // Turn everything back ON (Motor + LED)
      digitalWrite(led, HIGH);
    }
  }
}
