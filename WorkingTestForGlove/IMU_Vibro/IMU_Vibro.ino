// библиотека для работы с модулями IMU
#include <TroykaIMU.h>
#include "BluetoothSerial.h"
#include <Servo.h>

Servo left25;
Servo right26;

Servo left27;
Servo right14;
 
// множитель фильтра
#define BETA 0.22f
 
// создаём объект для фильтра Madgwick
Madgwick filter;
 
// создаём объект для работы с акселерометром
Accelerometer accel;
// создаём объект для работы с гироскопом
Gyroscope gyro;

BluetoothSerial SerialBT;
String data;
 
// переменные для данных с гироскопов, акселерометров
float gx, gy, gz, ax, ay, az;
 
// получаемые углы ориентации
float yaw, pitch, roll;
 
// переменная для хранения частоты выборок фильтра
float fps = 100;
 
void setup()
{
  // открываем последовательный порт
  Serial.begin(115200);
  SerialBT.begin("V-Arm");
  // инициализация акселерометра
  accel.begin();
  // инициализация гироскопа
  gyro.begin();
  // выводим сообщение об удачной инициализации
  filter.begin();
   left25.attach(25);
  right26.attach(26);
  left27.attach(27);
  right14.attach(14);
}
 
void loop()
{
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
        left25.write(74);
        right26.write(82);

        delay(100);
       
        left25.write(84);
        right26.write(82);
        break;
      case '6':
        left25.write(98);
        right26.write(106);

        delay(100);
        
        left25.write(98);
        right26.write(96);
        break;

     
      case '4':
        left27.write(90);
        right14.write(90);
        break;
      case '3':
        left27.write(84);
        right14.write(96);
        break;
      case '7':
        left27.write(76);
        right14.write(80);

        delay(100);
        
        left27.write(76);
        right14.write(90);
        break;
      case '8':
        left27.write(100);
        right14.write(104);

        delay(100);
        
        left27.write(90);
        right14.write(104);
        break;
    }
  } 
  // запоминаем текущее время
  unsigned long startMillis = millis();
 
    // считываем данные с акселерометра в единицах G
    accel.readAccelerationGXYZ(ax, ay, az);
    // считываем данные с гироскопа в радианах в секунду
    gyro.readRotationRadXYZ(gx, gy, gz);
  // устанавливаем коэффициенты фильтра
  //filter.setKoeff(fps, BETA);
   // Устанавливаем частоту фильтра
    filter.setFrequency(fps);
  // обновляем входные данные в фильтр
  filter.update(gx, gy, gz, ax, ay, az);
 
  // получение углов yaw, pitch и roll из фильтра
  yaw =  filter.getYawDeg();
  pitch = filter.getPitchDeg();
  roll = filter.getRollDeg();

  float q0,q1,q2,q3;
  filter.readQuaternions(&q0,&q1,&q2,&q3);
  // выводим полученные углы в serial-порт
 String data = String(q0) + "," + String(q1) + "," + String(q2) + "," + String(q3) + "," +(String)analogRead(33)+ "," +(String)analogRead(32)+","+(String)analogRead(35)+","+(String)analogRead(34)+","+(String)analogRead(39);
 //String data = String(q0) + "," + String(q1) + "," + String(q2) + "," + String(q3) + "," + "1000" + "," + "1000" + ","+ "1000" +","+ "1000" +","+ "1000";

Serial.println(data);
SerialBT.println(data);
  // вычисляем затраченное время на обработку данных
  unsigned long deltaMillis = millis() - startMillis;
  // вычисляем частоту обработки фильтра
  fps = 1000 / deltaMillis;
  
}
