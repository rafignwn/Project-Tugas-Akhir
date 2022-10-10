#include <ESP8266WiFi.h>
#include <WiFiClientSecure.h> 
#include <ESP8266WebServer.h>
#include <ESP8266HTTPClient.h>

// konfigurasi sensor pH
#define sensorPh A0
const int sample = 10;
float adc_resolution = 1024.0;

// nilai kalibrasi tegangan untuk ph mikrokontroler nodemcu
float PH4 = 3.30;
float PH7 = 2.83;

float PH_step;
// end configuration

// konfigurasi relay
#define pinKipas D4

// konfigurasi modul rtc
// CONNECTIONS:
// DS1307 SDA --> D2
// DS1307 SCL --> D1
// DS1307 VCC --> 5v
// DS1307 GND --> GND

#include <Wire.h> // must be included here so that Arduino library object file references work
#include <RtcDS1307.h>
RtcDS1307<TwoWire> Rtc(Wire);
/* for normal hardware wire use above */

bool waktunyaMakan = true;
bool firstLoop = true;
// end konfigurasi modul rtc

// konfigurasi sensor ultrasonik
// untuk menghitung sisa pakan
#define triggerPin  D0
#define echoPin     D7
#define TINGGI_WADAH_PAKAN 33  // satuan cm
// end ultrasonic

/* konfigurasi untuk servo */
#include <Servo.h>
Servo servoUye;
Servo servoGede;

#define PIN_SERVO_KATUP D8
#define PIN_SERVO_GEDE D3
#define MAX_PULSE 2400
#define MIN_PULSE 544
// end servo

/*  konfigurasi untuk timbangan / load cell
 *  pin data di pin D5 nodemcu
 *  pin clk di pin D6 nodemcu
 */
#include "HX711.h" 
 
#define DOUT  D5
#define CLK  D6
HX711 scale(DOUT, CLK);
 
float calibration_factor = 450.00; // nilai kalibrasi
float weight;
/* end konfigurasi timbangan */

/* Set these to your desired credentials. */
const char *ssid = "Redmioyoy";  //ENTER YOUR WIFI SETTINGS
const char *password = "12345678";
// set token api
#define tokenAPI "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuaW0iOjE5MDQwMTc5LCJuYW1lIjoiUmFmaSBndW5hd2FuIiwia2VuZGFyYWFuIjoiU2VwZWRhIE1vdG9yIEFzdHJlYSJ9.nS--cLfPwWcszhrETlrKcv6MT3hy0ZUCDntN8lZOlTY"

//Link to read data from https://primus.somee.com/checkTimeTOFeed
//Web/Server address to read/write from 
const char *host = "primus.somee.com";
const int httpsPort = 443;  //HTTPS= 443 and HTTP = 80

//SHA1 finger print of certificate use web browser to view and copy
const char fingerprint[] PROGMEM = "32 2D A5 7C 22 3C 04 C3 4B 2C 05 6D C4 3F 5F 88 64 BE 1C 9B";
//=======================================================================
//                    Power on setup
//=======================================================================

void setup() {
  Serial.begin(115200);
  delay(1000);
  // inisialisasi pin servo 
  servoUye.attach(PIN_SERVO_KATUP, MIN_PULSE, MAX_PULSE);
  servoUye.write(0);
  servoGede.attach(PIN_SERVO_GEDE, MIN_PULSE, MAX_PULSE);
  servoGede.write(0);
  delay(500);
  // end inisialisai servo

  // inisialisasi pin sensor pH 
  pinMode(sensorPh, INPUT);
  PH_step = (PH4 - PH7) / 3;
  delay(500);
  // end

  // inisialisasi pin relay
  pinMode(pinKipas, OUTPUT);
  digitalWrite(pinKipas, HIGH);
  // end inisialisasi

  // inisialisasi modul rtc
  Serial.print("compiled: ");
  Serial.print(__DATE__);
  Serial.println(__TIME__);
 
  Rtc.Begin();
  RtcDateTime compiled = RtcDateTime(__DATE__, __TIME__);
  printDateTime(compiled);
  Serial.println();
  if (!Rtc.IsDateTimeValid())
  {
      Serial.println("RTC lost confidence in the DateTime!");
      Rtc.SetDateTime(compiled);
  }
  if (!Rtc.GetIsRunning())
  {
      Serial.println("RTC was not actively running, starting now");
      Rtc.SetIsRunning(true);
  }
  RtcDateTime now = Rtc.GetDateTime();
  if (now < compiled)
  {
      Serial.println("RTC is older than compile time!  (Updating DateTime)");
      Rtc.SetDateTime(compiled);
  }
  else if (now > compiled)
  {
      Serial.println("RTC is newer than compile time. (this is expected)");
  }
  else if (now == compiled)
  {
      Serial.println("RTC is the same as compile time! (not expected but all is fine)");
  }
  Rtc.SetSquareWavePin(DS1307SquareWaveOut_Low);
  // end inisialsasi modul rtc  

  // inisialisasi pin sensor ultrasonic
  pinMode(triggerPin, OUTPUT);
  pinMode(echoPin, INPUT);
  // end inisialisasi sensor ultrasonic

  // inisialisasi timbangan
  scale.set_scale();
  scale.tare(); //Reset the scale to 0
  // end inisialisasi timbangan
  WiFi.mode(WIFI_OFF);        //Prevents reconnection issue (taking too long to connect)
  delay(1000);
  WiFi.mode(WIFI_STA);        //Only Station No AP, This line hides the viewing of ESP as wifi hotspot
  
  WiFi.begin(ssid, password);     //Connect to your WiFi router
  Serial.println("");

  Serial.print("Connecting");
  // Wait for connection
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  //If connection successful show IP address in serial monitor
  Serial.println("");
  Serial.print("Connected to ");
  Serial.println(ssid);
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());  //IP address assigned to your ESP
}

//=======================================================================
//                    Main Program Loop
//=======================================================================
void loop() {
  // deklarasi variable yang dibutuhkan
  String cekPakan, Link_tambah_data;
  String linkTambahData;
  int pengukuran;

  for(int i = 0; i < sample; i++) {
    pengukuran += analogRead(sensorPh);
    delay(10);
  }
  // pengukuran = analogRead(sensorPh);
  Serial.println(pengukuran/sample);
  float voltage = 3.3 /adc_resolution * pengukuran/sample;
  Serial.print("voltage : ");
  Serial.println(voltage);
  Serial.print("Nilai pH : ");
  Serial.println(nilai_ph(voltage));

  //GET Data
  float nilaiPh =  nilai_ph(voltage);  //
  // membaca nilai pH
  linkTambahData = String("/addPhValue/") + nilaiPh + "/";
  requests(linkTambahData, tokenAPI);
  delay(1000);  // kirim data setiap 5 detik
  
  // membaca nilai sisa pakan
  Serial.print("Sisa Pakan :");
  Serial.print(ambilSisaPakan());
  Serial.println(" %");
  
  if (firstLoop) {
    int sisaPakan = ambilSisaPakan();
    Serial.println("Update Sisa Pakan");
    // membaca nilai sisa pakan
    Serial.print("Sisa Pakan :");
    Serial.print(sisaPakan);
    Serial.println(" %");
    requests(String("/updateSisaPakan/") + sisaPakan + "/", tokenAPI);
    firstLoop = false;
  }

  // cek makan dengan modul rtc
  // cek rtc
  if (!Rtc.IsDateTimeValid())
  {
      Serial.println("RTC lost confidence in the DateTime!");
  }
  RtcDateTime now = Rtc.GetDateTime();
  printDateTime(now);
  int pukul = jam(now);
  // cek apa sudah mendekati waktu makan?
  if(pukul == 7 || pukul == 15) {
    waktunyaMakan = true;
  }
  // cek waktu makan
  waktunyaMakan = waktunyaMakan ? waktuMakan(now) : waktunyaMakan;
  // jika waktunya makan
  // maka makanlah
  if(waktunyaMakan) {
    Serial.println("Waktunya Makan");
    fungsiMemberiPakan();
    waktunyaMakan = false;
  }
  Serial.println();
  delay(1000);
  // end cek makan dengan modul rtc  

  // mengecek paakan
  cekPakan = requests("/checkTimeToFeed/", "");
  // cek apakan kondisi pakan '1' atau '0'
  if (cekPakan == "1") {
      fungsiMemberiPakan();
  }
  Serial.println("Kondisi Pakan : " + cekPakan);
//  delay(2000);  //GET Data at every 2 seconds
}

// fungsi untuk menghitung niai pH
float nilai_ph(float voltage) {
  return 7.00 + ((PH7 - voltage) / PH_step);
}

// Membuat fungsi untuk request data
String requests(String Link, String token) {
  WiFiClientSecure httpsClient;    //Declare object of class WiFiClient

  Serial.println(host);

  Serial.printf("Using fingerprint '%s'\n", fingerprint);
  httpsClient.setFingerprint(fingerprint);
  httpsClient.setTimeout(1000); // 15 Seconds
  delay(1000);
  
  Serial.println("HTTPS Connecting");
  bool connectedToWeb = httpsClient.connect(host, httpsPort);
//  httpsClient.setTimeout(500);
  // jika gagal terhubung ke web maka hubungkan ulang
  int r=0; //retry counter
  if(!connectedToWeb) {
    while((!httpsClient.connect(host, httpsPort)) && (r < 30)){
        delay(50);
        Serial.print(".");
        r++;
    }
  }
  if(r==30) {
    Serial.println("Connection failed");
  }
  else {
    Serial.println("Connected to web");
  }

  Serial.print("requesting URL: ");
  Serial.println(host+Link);

  httpsClient.print(String("GET ") + Link + token + " HTTP/1.1\r\n" +
               "Host: " + host + "\r\n" +               
               "Connection: close\r\n\r\n");

  Serial.println("request sent");
                  
  while (httpsClient.connected()) {
    String line = httpsClient.readStringUntil('\n');
    if (line == "\r") {
      Serial.println("headers received");
      break;
    }
  }

  Serial.println("reply was:");
  Serial.println("==========");
  String line = "";
  while(httpsClient.available()){        
    line = httpsClient.readStringUntil('\n');
    httpsClient.setTimeout(500);//Read Line by Line
    Serial.println(line); //Print response
  }
  Serial.println("==========");
//  httpsClient.close();
  Serial.println("closing connection");
  return line;
}

// membuat fungsi untuk memberi pakan ikan
void beriPakan() {
  buka(180);
  tutup(180);
}

void buka(int drajat) {
  for(int i = 0; i <= drajat; i+=2) {
    servoUye.write(i);
//    delay buka pakan
    delay(2);
  }
}

void tutup(int drajat) {
  servoUye.write(drajat);
  delay(500);
  servoUye.write(0);
  delay(500);
}
// fungsi untuk mengambil nilai sisa pakan
int ambilSisaPakan() {
  // mengambil nilai jarak dengan sensor ultrasonik
  long duration, jarak;
  digitalWrite(triggerPin, LOW);
  delayMicroseconds(2); 
  digitalWrite(triggerPin, HIGH);
  delayMicroseconds(10); 
  digitalWrite(triggerPin, LOW);
  duration = pulseIn(echoPin, HIGH);
  jarak = (duration/2) / 29.1;
  Serial.print("Jarak : ");
  Serial.println(jarak);
  delay(1000);

  // hitung nilai sisa pakan
  int nilaiSisaPakan = ((TINGGI_WADAH_PAKAN - jarak) * 100 / TINGGI_WADAH_PAKAN) + 10;
  // cek nilai negatif 
  // jika hasil negatif bulatkan ke 0
  nilaiSisaPakan = nilaiSisaPakan < 0 ? 0 : nilaiSisaPakan;
  // mengembalikan nilai sisa pakan
  return nilaiSisaPakan;
}

// fungsi modul rtc
#define countof(a) (sizeof(a) / sizeof(a[0]))
void printDateTime(const RtcDateTime& dt)
{
    Serial.print(dt.Day());
    Serial.print("/");
    Serial.print(dt.Month());
    Serial.print("/");
    Serial.println(dt.Year());
    Serial.print(dt.Hour());
    Serial.print(":");
    Serial.print(dt.Minute());
    Serial.print(":");
    Serial.println(dt.Second());
}

int jam (const RtcDateTime& dt) {
  return dt.Hour();
}

bool waktuMakan(const RtcDateTime& dt) {
  if (dt.Hour() == 8 && dt.Minute() >= 15) {
    return true;
  }  else if (dt.Hour() == 16 && dt.Minute() >= 30) {
    return true;
  }
  return false;
}
// end fungsi modul rtc

void fungsiMemberiPakan() {
  int sisaPakan;
  float batasBeratPakan = 100.0, beratPakan;
  while(true) {
    // memberi pakan
    beriPakan();
    // delay 1.5 detik menunggu pakan turun ke timbangan
    delay(1500);
    Serial.println("Sedang Memberi Pakan");
    scale.set_scale(calibration_factor); //Adjust to this calibration factor
    weight = scale.get_units(), 4;
    weight -= 2.0;
    if (weight <= 0) {
      weight = 0;
    }
   
    Serial.print("Berat Pakan : ");
    Serial.print(weight);
    Serial.println(" gram");
    delay(500);
    //mengambil nilai timbangan
    beratPakan = weight; 
    // sisaPakan mengambil dari perhitungan sensor ultrasonic dengan tinggi wadah pakan, hasil dalam bentuk persen
    sisaPakan = ambilSisaPakan(); 
    // keluar looping jika pakan sudah cukup
    if (beratPakan >= batasBeratPakan) break;
  }
  servoGedeMuter();
  delay(1000);
  // menyalakan relay 
  nyalakanKipas();
  // update nilai kondisi pakan di database menjadi 0
  // afterFeed/{kondisi}/{berat_pakan}/{sisa_pakan}/{token}
  String linkUpdatePakan = String("/afterFeed/0/") + beratPakan + "/" + sisaPakan + "/";
  requests(linkUpdatePakan, tokenAPI);
  Serial.println("Selesai Memberi Pakan");
}

void servoGedeMuter() {
  for (int x = 0; x <= 180; x+=2) {
    servoGede.write(x);
    delay(50);
  }
  delay(1000);
  servoGede.write(0);
  delay(1000);
}

void nyalakanKipas() {
  digitalWrite(pinKipas, LOW);
  delay(10000);
  digitalWrite(pinKipas, HIGH);
}
