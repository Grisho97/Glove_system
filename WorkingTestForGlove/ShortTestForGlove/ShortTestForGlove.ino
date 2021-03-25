String data;
void setup()
{
  // открываем последовательный порт
  Serial.begin(115200);
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
}
