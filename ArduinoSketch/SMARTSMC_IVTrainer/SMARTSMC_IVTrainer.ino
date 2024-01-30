/*
 * This must go into a folder with the same name in your Arduino projects.
 * Last Modified 10/24/17 (DL reviewed AB's implementation.)
 * 
  Three Arduino outputs, several Arduino Inputs

  Requires an initial handshake
  Does not stream data; sends data upon request.

  These outputs are continously streamed by default in one line:
  one output reports two force sensitive resistors on the Ultrasound Probe tip
  one output reports the TUI_Button shutter button

  two input commands for the Hub_Thumper motor
  two input commands for the 5V Parker microvalve
  three input commands for the syringe LED

  Hub_Thumper:
  for a membrane pop, use a 8ms to 16ms pulse
  for texture, ON for 1-15ms and OFF for 15-100ms
  recode Hub_Thumper so there is no delay() used

  Microvalve:
  a solenoid valve can be driven off the board using a Meder reed relay

  Ultrasound:
  two Force Sensitive Resistors measures pressure on the US probe.
  FSR 1 = the side of the probe with the index notch.
  a resistor can be sized to the circuit to that:
  0: probe is not touching skin.
  20: probe is touching skin enough for an image
  50 to 100: probe is pushing hard enough to noticeably compress veins
  255: highest value possible.  We don't expect the probe to be pushed this hard into the skin.
  OR
  no resistor can be used,
  and 20 and above is enough for an image.

  Digital IO pins available on the board: some have an extra function.

  Pulse Width Modulation possible, good for LED colors and tactile units:
  3
  5
  6
  9
  10
  11

  These are good for only on and off, such as the TUI button:
  2
  4
  7
  8
  12


*/

const int TUI_Button  = 4;     // connected to the TUI_Button shutter button.  Normally open only.
const int ValvePressureSensor = 2;  // 2 Analog In pin A2 is connected to the syringe valve pressure sensor.
const int USProbeFSR_NotchSide = 4; // 4 Analog In pin A4 is connected to the FSR on the notch side of the probe.
const int USProbeFSR_FlatSide = 3;  // 3 Analog In pin A3 is connected to the FSR on the flat side of the probe.
const int Hub_LED_Red   = 9;  // old = 5; new = 9;    // connected to the multicolor LED.  common Cathode.
const int Hub_LED_Green = 10; // old = 6; new = 10;   // connected to the multicolor LED.  common Cathode.
const int Hub_LED_Blue  = 11; // old = 7; new = 11;   // connected to the multicolor LED.  common Cathode.
const int Hub_Thumper = 3;     // vibration motor can be driven straight from the board. Digital IO pin 3 (PWM possible but not used yet)
const int SyringeValve = 2;    // drives a relay that controls a Clippard microvalve that goes to the syringe.
const int SerialComm_OK = 13;  // this is an "OK light" for the white box. drives an LED that lights up when the Serial.available = TRUE
//IV SPECIFIC
const int IVT_TourniquetSensor = 0; // 0 Analog In pin A0 is connected to the IV Trainer tourniquet FSRs.
const int IVT_SlapSensor = 1; // 1 Analog In pin A1 is connected to the IV Trainer wrist slap FSR.
const int IVT_TractionSensor = 5; // 5 Analog In pin A5 is connected to the IV Trainer finger traction FSRs.

float ValvePressure = 0;
int USProbePressure_NotchSide = 0;
int USProbePressure_FlatSide = 0;
int TUI_ButtonShutterValue = 0;

// IV SPECIFIC
// NOTE, we may have to detect wrist slap by rise time here (quicker code) rather than in Unity (slower code dependant on framerate)
long TourniquetPressure;
long SlapPressure;
long TractionPressure;

int ReadByte = 0; // simple input communication uses a single char.
String ReportString;

// Timer properties for streaming data out at a set rate
// We now only send data upon receiving a request from an outside entity.
//boolean TimeToSend = false;
//unsigned long PreviousSentMillis = 0;
//long SendInterval = 20;

// Boolean for determining when to send data
boolean sendRequestReceived = false;

// Timer properties for flashing the SerialComm_OK light, signifing power but no Unity connection.
boolean TimeToFlash = false;
unsigned long PreviousFlashMillis = 0;
unsigned long FlashInterval = 500;
boolean TimeToFlashStrobe = false;

// Syringe Pressure Sensor variables
int ValvePressureZero;
long ValvePressureMeasurementCount;
long ValvePressureAccumulator;

// testing variable
int TestSequence = 0;

bool handShakeDone = false;
/*long testMillisFirstHandshake = 0;
long testMillisSecondHandshake = 0;
long testMillisFirstHandshakePrevious = 0;
long testMillisSecondHandshakePrevious = 0;
long testMillisLoopPrevious = 0;
long testMillisLoopCurrent = 0;
bool testCommandSend = false;
long testFTSR = 0;
long testTTSR = 0;*/

//IV SPECIFIC
bool IVTrainer = false;

void setup()
{
  Serial.begin(19200);  // initialize serial communication at x bits per second:
  pinMode  (SerialComm_OK,   OUTPUT);
  pinMode  (SyringeValve,   OUTPUT);
  pinMode  (TUI_Button,    INPUT_PULLUP);
  pinMode  (Hub_Thumper,   OUTPUT);
  pinMode  (Hub_LED_Red,   OUTPUT);
  pinMode  (Hub_LED_Green, OUTPUT);
  pinMode  (Hub_LED_Blue,  OUTPUT);

  digitalWrite(SyringeValve, HIGH);
  digitalWrite (Hub_Thumper, LOW);

  digitalWrite (Hub_LED_Red,   LOW);
  digitalWrite (Hub_LED_Green, LOW);
  digitalWrite (Hub_LED_Blue,  LOW);

  // this LED is part of the whitebox front end
  digitalWrite(SerialComm_OK, HIGH);

  // TEST
  //Serial.print("Starting up: ");
 // Serial.print(millis());
  //Serial.println();
  // END TEST
  StartupTest();
  
  // TEST
 // /Serial.print("Startup complete: ");
 // Serial.print(millis());
  //Serial.println();
  // END TEST
}


void loop()
{
  //boolean testBoolean = false;
  //boolean testBoolean2 = false;

  unsigned long currentMillis = millis();
  //Serial.println(PreviousFlashMillis);
  //Serial.println(currentMillis);
  //Serial.println();
  // FLASH LED IF NO UNITY INFO RECEIVED -------------------------------------------
  // is it time to flash the LED?
  if (currentMillis - PreviousFlashMillis > FlashInterval) {
  // TEST
  //Serial.print("WTF: ");
  //Serial.print(millis());
  //Serial.println();

    /*if(handShakeDone)
    {
      testMillisLoopPrevious = PreviousFlashMillis;
      testMillisLoopCurrent = currentMillis;
    }*/
  
  // END TEST
    PreviousFlashMillis = currentMillis;
    TimeToFlash = true;
    digitalWrite(Hub_LED_Red, LOW);
    digitalWrite(Hub_LED_Green, LOW);
    digitalWrite(Hub_LED_Blue, LOW);
    digitalWrite(SyringeValve, LOW);
    digitalWrite(Hub_Thumper, LOW);
    handShakeDone = false;
    sendRequestReceived = false;
    
    /*
    
    //TEST
    if(currentMillis - PreviousFlashMillis > 5000)
      digitalWrite(Hub_LED_Red, HIGH);
    digitalWrite(Hub_LED_Blue, HIGH);
    delay(5000);
    digitalWrite(Hub_LED_Blue, LOW);
    digitalWrite(Hub_LED_Red, LOW);
    currentMillis = millis();
    PreviousFlashMillis = currentMillis;
    //END TEST


    */
  }
  //TEST
  //else
  //{
    //digitalWrite(Hub_LED_Blue, HIGH);
    //delay(5000);
    //currentMillis = millis();
    //PreviousFlashMillis = currentMillis;
  //}
  //END TEST
  

  if (TimeToFlash) {
    TimeToFlashStrobe = !TimeToFlashStrobe;
    if (TimeToFlashStrobe) {
      digitalWrite(SerialComm_OK, LOW);
    } else {
      digitalWrite(SerialComm_OK, HIGH);
    }
    TimeToFlash = false;
  }

  if (handShakeDone)
  {
  // TEST
  //Serial.print("Handshake done, in loop: ");
  //Serial.print(millis());
  //Serial.println();
  // END TEST
    // ACCUMULATE TUI BUTTON PRESSES --------------------------------------------------
    // look for any TUI button presses between since the last data send.
    int buttonState = digitalRead(TUI_Button);
    if (buttonState == HIGH) {
      TUI_ButtonShutterValue = 2;
    }

    // ACCUMULATE FSR VALUES PRESSES --------------------------------------------------
    // look for the highest USProbeSensor FSR values between since the last data send.
    int fsr1 = analogRead(USProbeFSR_NotchSide);
    if (fsr1 > USProbePressure_NotchSide) USProbePressure_NotchSide = fsr1;
    int fsr2 = analogRead(USProbeFSR_FlatSide);
    if (fsr2 > USProbePressure_FlatSide) USProbePressure_FlatSide = fsr2;

    //IV SPECIFIC
    if (IVTrainer) {
      // look for the highest FSR values between since the last data send.
      int fsr3 = analogRead(IVT_TourniquetSensor);
      if (fsr3 > TourniquetPressure) TourniquetPressure = fsr3;
      int fsr4 = analogRead(IVT_SlapSensor);
      if (fsr4 > SlapPressure) SlapPressure = fsr4;
      int fsr5 = analogRead(IVT_TractionSensor);
      if (fsr5 > TractionPressure) TractionPressure = fsr5;
    }


    // ACCUMULATE SYRINGE PRESSURE MEASUREMENTS ---------------------------------------
    // aggregate the raw values

    // ValvePressureAccumulator
    ValvePressureMeasurementCount ++;
    ValvePressureAccumulator += analogRead(ValvePressureSensor);// - ValvePressureZero;



    // Auxiliary Data Sets, not used, random implementation below

    float auxDataA = 101;
    float auxDataB = ValvePressureAccumulator/200;
    float auxDataC = fsr2*1.5;
    float auxDataD = fsr1*-1;
    float auxDataE = auxDataA/auxDataB*auxDataC;





















   
    // SEND DATA EVERY SO MANY MILLISECONDS -------------------------------------------
    // is it time to send data?
    // No longer in use. We now only send data on request.

    // FLASH LED IF NO UNITY INFO RECEIVED -------------------------------------------
    // is it time to flash the LED?
    if (currentMillis - PreviousFlashMillis > FlashInterval) {
      PreviousFlashMillis = currentMillis;
      TimeToFlash = true;
      digitalWrite(Hub_LED_Red, LOW);
      digitalWrite(Hub_LED_Green, LOW);
      digitalWrite(Hub_LED_Blue, LOW);
      digitalWrite(SyringeValve, LOW);
      digitalWrite(Hub_Thumper, LOW);
    }

    if (TimeToFlash) {
      TimeToFlashStrobe = !TimeToFlashStrobe;
      if (TimeToFlashStrobe) {
        digitalWrite(SerialComm_OK, LOW);
      } else {
        digitalWrite(SerialComm_OK, HIGH);
      }
      TimeToFlash = false;
    }


    // SEND DATA UPON REQUEST FROM OUTSIDE .- -. -.. .-. . / -... .. --. --- ...
    if (/*TimeToSend*/sendRequestReceived) {
      sendRequestReceived = false;
      // upload TUI and US Probe pressures to the send buffer
      // transmits on Arduino are not buffered so there is no need to flush()


      // first, find the average pressure values and report those
      ValvePressure = 0;
      if (ValvePressureMeasurementCount > 0) {
        ValvePressure = (ValvePressureAccumulator / (float)ValvePressureMeasurementCount) - ValvePressureZero;
        ValvePressureAccumulator = 0;
        ValvePressureMeasurementCount = 0;
      }
      
      //IV SEPCIFIC STRUCTURE
      if (IVTrainer) {
      // note, Serial.print eventually calls a Serial.write
      Serial.print("$");
      Serial.print(TUI_ButtonShutterValue);
      Serial.print(",");
      Serial.print(USProbePressure_NotchSide);
      Serial.print(",");
      Serial.print(USProbePressure_FlatSide);
      Serial.print(",");
      //Serial.print(ValvePressure);
      //Serial.print(",");
      Serial.print(TourniquetPressure);
      Serial.print(",");
      Serial.print(SlapPressure);
      Serial.print(",");
      Serial.print(TractionPressure);
      Serial.print("#");
      Serial.println();
      }
      else
      {


      // note, Serial.print eventually calls a Serial.write
      Serial.print("$");
      Serial.print(TUI_ButtonShutterValue);
      Serial.print(",");
      Serial.print(USProbePressure_NotchSide);
      Serial.print(",");
      Serial.print(USProbePressure_FlatSide);
      Serial.print(",");
      Serial.print(ValvePressure);
        //TEST DATA
        /*Serial.print(",");
        Serial.print(testMillisFirstHandshake);
        Serial.print(","); 
        Serial.print(testMillisFirstHandshakePrevious);
        Serial.print(",");
        Serial.print(testMillisSecondHandshake);
        Serial.print(","); 
        Serial.print(testMillisSecondHandshakePrevious);
        Serial.print(","); 
        Serial.print(testMillisLoopPrevious);
        Serial.print(","); 
        Serial.print(testMillisLoopCurrent);
        Serial.print(",");
        Serial.print(testCommandSend);
        Serial.print(",");
        Serial.print(testFTSR);
        Serial.print(",");
        Serial.print(testTTSR);*/
        //END TEST


/////// AUX DATA INPUTS
      //Serial.print(",");
      //Serial.print(auxDataA);
      //Serial.print(",");
      //Serial.print(auxDataB);
      //Serial.print(",");
      //Serial.print(auxDataC);
      //Serial.print(",");
      //Serial.print(auxDataD);
      //Serial.print(",");
      //Serial.print(auxDataE);
      
      Serial.print("#");
      Serial.println();
      } 

      // reset TUI button presses
      if (TUI_ButtonShutterValue > 0)
        TUI_ButtonShutterValue--;

      // reset USProbeSensor valuse
      USProbePressure_NotchSide = 0;
      USProbePressure_FlatSide = 0;

      //IV SPECIFIC
      TourniquetPressure = 0;
      SlapPressure = 0;
      TractionPressure = 0;

      // reset Data Send Flag
      //TimeToSend = false;
    }
  }


}


// Timer properties for flashing the SerialComm_OK light, signifing and active Unity connection
// boolean TimeToFlash = false;
// unsigned long PreviousFlashMillis = 0;
// long FlashInterval = 500;

void serialEvent() {

  ReadByte = 0;
  while (Serial.available()) {

    ReadByte = Serial.read();

    switch (ReadByte)
    {

      case 65: // ASCII "A"
      CommandA();
        break;

      case 66: // ASCII "B"
      CommandB();
        break;

      case 67: // ASCII "C"
      CommandC();
        break;

      case 68: // ASCII "D"
      CommandD();
        break;

      case 69: // ASCII "E"
      CommandE();
        break;

      case 70: // ASCII "F"
      CommandF();
        break;

      case 71: // ASCII "G"
      CommandG();
        break;

      case 72: // ASCII "H"
      CommandH();
        break;

      case 73: // ASCII "I"
      CommandI();
        break;

      case 74: // ASCII "J"
      CommandJ();
        break;

      case 75: // ASCII "K"
      CommandK();
        break;

      case 76: // ASCII "L"
      CommandL();
        break;

      case 77: // ASCII "M"
      CommandM();
        break;

      case 78: // ASCII "N"
      CommandN();
        break;

      case 79: // ASCII "O"
      CommandO();
        break;

      case 80: // ASCII "P"
      CommandP();
        break;

      case 81: // ASCII "Q"
      CommandQ();
        break;

      case 82: // ASCII "R"
      CommandR();
        break;

      case 83: // ASCII "S"
      CommandS();
        break;

      case 84: // ASCII "T"
      CommandT();
        break;

      case 85: // ASCII "U"
      CommandU();
        break;

      case 86: // ASCII "V"
      CommandV();
        break;

      case 87: // ASCII "W"
      CommandW();
        break;

      case 88: // ASCII "X"
      CommandX();
        break;

      case 89: // ASCII "Y"
      CommandY();
        break;

      case 90: // ASCII "Z"
      CommandZ();
        break;

      case 48: // ASCII "0"
      Command0();
        break;

      case 49: // ASCII "1"
      Command1();
        break;

      case 50: // ASCII "2"
      Command2();
        break;

      case 51: // ASCII "3"
      Command3();
        break;

      case 52: // ASCII "4"
      Command4();
        break;

      case 53: // ASCII "5"
      Command5();
        break;

      case 54: // ASCII "6"
      Command6();
        break;

      case 55: // ASCII "7"
      Command7();
        break;

      case 56: // ASCII "8"
      Command8();
        break;

      case 57: // ASCII "9"
      Command9();
        break;

      case 45: // ASCII "-"
      CommandHandshake();
        break;

      //IV SPECIFIC
      case 61 : // ASCII "="
      CommandHandshakeIV();
        break;
        
      case 42: // ASCII "*";
      CommandSendRequest();
        break;
        
      default:
        break;

    }

  }


}
void CommandHandshake()
{
  Serial.flush();
  delay(2);
  Serial.print(">");
  Serial.flush();
  handShakeDone = true;
  UnityOK();
  IVTrainer = false;
  /*if(testMillisFirstHandshake==0)
  {
    testMillisFirstHandshake = millis();
    testMillisFirstHandshakePrevious = PreviousFlashMillis;
  }
  else
  {
    testMillisSecondHandshake = millis();
    testMillisSecondHandshakePrevious = PreviousFlashMillis;
  }*/
  delay(2);
}
//IV SPECIFIC
void CommandHandshakeIV()
{
  Serial.flush();
  delay(2);
  Serial.print(">");
  Serial.flush();
  handShakeDone = true;
  UnityOK();
  IVTrainer = true;
  /*if(testMillisFirstHandshake==0)
  {
    testMillisFirstHandshake = millis();
    testMillisFirstHandshakePrevious = PreviousFlashMillis;
  }
  else
  {
    testMillisSecondHandshake = millis();
    testMillisSecondHandshakePrevious = PreviousFlashMillis;
  }*/
  delay(2);
}
void CommandSendRequest()
{
  // SMARTS Unity program is ready for your data.
  sendRequestReceived = true;
  /*if(testMillisSecondHandshake == 0)
  {
    testCommandSend = true;
    if(testFTSR == 0)
      testFTSR = millis();
    testTTSR++;
  }*/
  UnityOK();
}
void UnityOK() {
  unsigned long currentMillis = millis();
  PreviousFlashMillis = currentMillis;
  digitalWrite(SerialComm_OK, HIGH);
}

void CommandA(){/*DO NOTHING*/}
void CommandB(){/*DO NOTHING*/}
void CommandC(){/*DO NOTHING*/}
void CommandD(){/*DO NOTHING*/}
void CommandE(){/*DO NOTHING*/}
void CommandF(){/*DO NOTHING*/}
void CommandG(){/*DO NOTHING*/}
void CommandH(){/*DO NOTHING*/}
void CommandI(){/*DO NOTHING*/}
void CommandJ(){/*DO NOTHING*/}
void CommandK(){/*DO NOTHING*/}
void CommandL(){/*DO NOTHING*/}
void CommandM(){/*DO NOTHING*/}
void CommandN(){/*DO NOTHING*/}
void CommandO(){/*DO NOTHING*/}
void CommandP()
{
  digitalWrite(SyringeValve, HIGH);
  delay(30);
  ValvePressureZero = analogRead(ValvePressureSensor);
}
void CommandQ(){/*DO NOTHING*/}
void CommandR(){/*DO NOTHING*/}
void CommandS(){/*DO NOTHING*/}
void CommandT(){/*DO NOTHING*/}
void CommandU(){/*DO NOTHING*/}
void CommandV(){/*DO NOTHING*/}
void CommandW(){/*DO NOTHING*/}
void CommandX(){/*DO NOTHING*/}
void CommandY(){/*DO NOTHING*/}
void CommandZ(){/*DO NOTHING*/}
void Command0(){/*DO NOTHING*/}
void Command1()
{
  FeelAPop_Obvious();
}
void Command2()
{
  FeelAPop_Subtle();
}
void Command3()
{
  digitalWrite(SyringeValve, HIGH); // Allows plunger movement}
}
void Command4()
{
  digitalWrite(SyringeValve, LOW); // does not allow plunger movement
}
void Command5(){/*DO NOTHING*/}
void Command6()
{
  digitalWrite(Hub_LED_Green, HIGH);
}
void Command7()
{
  digitalWrite(Hub_LED_Blue, HIGH);
}
void Command8()
{
  digitalWrite(Hub_LED_Red, HIGH);
}
void Command9()
{
  digitalWrite (Hub_LED_Red,   LOW);
  digitalWrite (Hub_LED_Green, LOW);
  digitalWrite (Hub_LED_Blue,  LOW);
}

void StartupTest()
{

  //----- Hub_Thumper test: this feels like a heartbeat BTW
  FeelAPop_Obvious();
  delay(200);
  FeelAPop_Subtle();
  delay(400);
  FeelAPop_Obvious();
  delay(200);
  FeelAPop_Subtle();
  delay(400);
  FeelAPop_Obvious();
  delay(200);
  FeelAPop_Subtle();
  delay(400);


  //----- LOR Valve test:

  digitalWrite(SyringeValve, LOW);
  delay(100);
  digitalWrite(SyringeValve, HIGH);
  delay(100);
  digitalWrite(SyringeValve, LOW);
  delay(100);
  digitalWrite(SyringeValve, HIGH);
  delay(300);
  ValvePressureZero = analogRead(ValvePressureSensor);

  //----- Red LED test:
  digitalWrite(Hub_LED_Red, HIGH);
  delay(250);
  digitalWrite(Hub_LED_Red, LOW);

  //----- Blue LED test:
  digitalWrite(Hub_LED_Blue, HIGH);
  delay(250);
  digitalWrite(Hub_LED_Blue, LOW);
  delay(250);

  //----- Red LED test:
  digitalWrite(Hub_LED_Red, HIGH);
  digitalWrite(Hub_LED_Green, HIGH);
  digitalWrite(Hub_LED_Blue, HIGH);
  delay(250);
  digitalWrite(Hub_LED_Red, LOW);
  digitalWrite(Hub_LED_Blue, LOW);

  delay(1000);
  digitalWrite(Hub_LED_Green, LOW);
  PreviousFlashMillis = millis();
}



void FeelAPop_Variable(int d)
{
  digitalWrite(Hub_Thumper, HIGH);
  delay(d);
  digitalWrite(Hub_Thumper, LOW);
}



void FeelAPop_Obvious()
{
  digitalWrite(Hub_Thumper, HIGH);
  delay(20);
  digitalWrite(Hub_Thumper, LOW);
}


void FeelAPop_Subtle()
{
  digitalWrite(Hub_Thumper, HIGH);
  delay(14);
  digitalWrite(Hub_Thumper, LOW);
}
