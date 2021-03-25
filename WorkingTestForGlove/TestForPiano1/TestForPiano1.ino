void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);
}

void loop() {
  // put your main code here, to run repeatedly:
String data = (String)analogRead(34)+","+(String)analogRead(27)+ "," +(String)analogRead(33)+","+(String)analogRead(25)+","+(String)analogRead(26);
Serial.println(data);
}
