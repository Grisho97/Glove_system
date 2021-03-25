#include <Servo.h>

Servo left25;
Servo right26;
String data;
void setup()
{
  // открываем последовательный порт
  Serial.begin(115200);
  left25.attach(25);
  right26.attach(26);
}
void loop()
{
  float q0,q1,q2,q3;
  q0 = 0;
  q1 = 0;
  q3 = 0;
  q2 = 0;
   String data = String(q0) + "," + String(q1) + "," + String(q2) + "," + String(q3) + "," +(String)analogRead(39)+ "," +(String)analogRead(34)+","+(String)analogRead(35)+","+(String)analogRead(32)+","+(String)analogRead(33);
   Serial.println(data);

 if (Serial.available())
  {
  byte ledState = Serial.read();
  switch (ledState)
    {
      case '0':
        left25.write(84);
        right26.write(96);
        break;
      case '1':
        left25.write(90);
        right26.write(90);
        break;
      case '5':
        left25.write(84);
        right26.write(82);
        break;
      case '6':
        left25.write(98);
        right26.write(96);
        break;
    }
  } 
}

