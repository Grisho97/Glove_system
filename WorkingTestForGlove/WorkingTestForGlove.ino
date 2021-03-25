// библиотека для работы I²C
//#include <Wire.h>
// библиотека для работы с модулями IMU
#include <TroykaIMU.h>
#include "BluetoothSerial.h"
 
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
}
 
void loop()
{
  // запоминаем текущее время
  unsigned long startMillis = millis();
 
  // считываем данные с акселерометра в единицах G
  accel.readGXYZ(&ax, &ay, &az);
  // считываем данные с акселерометра в радианах в секунду
  gyro.readRadPerSecXYZ(&gx, &gy, &gz);
  // устанавливаем коэффициенты фильтра
  filter.setKoeff(fps, BETA);
  // обновляем входные данные в фильтр
  filter.update(gx, gy, gz, ax, ay, az);
 
  // получение углов yaw, pitch и roll из фильтра
  yaw =  filter.getYawDeg();
  pitch = filter.getPitchDeg();
  roll = filter.getRollDeg();

  float q0,q1,q2,q3;
  filter.readQuaternions(&q0,&q1,&q2,&q3);
  // выводим полученные углы в serial-порт
  String data = String(q0) + "," + String(q1) + "," + String(q2) + "," + String(q3) + "," +(String)analogRead(39)+ "," +(String)analogRead(34)+","+(String)analogRead(35)+","+(String)analogRead(32)+","+(String)analogRead(33);
//  String data = String(q0) + "," + String(q1) + "," + String(q2) + "," + String(q3) + "," +(String)analogRead(34)+ "," +(String)analogRead(33)+","+(String)analogRead(25)+","+(String)analogRead(26)+","+(String)analogRead(27);

Serial.println(data);
SerialBT.println(data);
  // вычисляем затраченное время на обработку данных
  unsigned long deltaMillis = millis() - startMillis;
  // вычисляем частоту обработки фильтра
  fps = 1000 / deltaMillis;
}
