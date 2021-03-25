using System;
using System.IO;
using UnityEngine;
using Random = System.Random;

class PerceptronExample {
    static readonly String[] GESTURE_FILES = new String[] {
       // "input/gesture1.bin",
      //  "input/gesture2.bin",
      //  "input/gesture3.bin"
      
      //@"C:\Users\Gregory\Desktop\gestures\gestureOkey.bin",
      //@"C:\Users\Gregory\Desktop\gestures\gesture1.bin",
      @"C:\Users\Gregory\Desktop\gestures\gestureThumb.bin",
      @"C:\Users\Gregory\Desktop\gestures\gestureHand.bin",
      //@"C:\Users\Gregory\Desktop\gestures\gestureKoza.bin",
      //@"C:\Users\Gregory\Desktop\gestures\gesturePhone.bin",
      @"C:\Users\Gregory\Desktop\gestures\gestureNewGun.bin",
      //@"C:\Users\Gregory\Desktop\gestures\gestureFist.bin",
      //@"C:\Users\Gregory\Desktop\gestures\gestureFack.bin",

    };

    static readonly int GESTURES_COUNT = GESTURE_FILES.Length;

    public static void Main() {
        learnExample(0.05f);
        useExample();
    }

    // Сеть будет учиться до тех пор, пока вероятность ошибки не снизится до strictness
    private static void learnExample(float strictness) {
        Perceptron perceptron = new Perceptron(GESTURES_COUNT);
        perceptron.teach(GESTURE_FILES, strictness);
        perceptron.saveToFile();
    }

    private static void useExample() {
        Perceptron perceptron = new Perceptron(GESTURES_COUNT);
        perceptron.loadFromFile();

        // smoke test -- загрузим те же данные и проверим, что сеть правильно распознала образы
        bool testResult = true;
        for (int i = 0; i < GESTURE_FILES.Length; i++) {
            BinaryReader binaryReader = new BinaryReader(new FileStream(GESTURE_FILES[i], FileMode.Open));
            int testOffset = 200;
            binaryReader.BaseStream.Seek(testOffset * Perceptron.DISTRIBUTION_LAYER_SIZE * sizeof(float), SeekOrigin.Begin);
            float[] row = new float [] {
                binaryReader.ReadSingle(),
                binaryReader.ReadSingle(),
                binaryReader.ReadSingle(),
                binaryReader.ReadSingle(),
                binaryReader.ReadSingle()
            };
            if (i != perceptron.recognize(row)) {
                Console.WriteLine("Smoke test: Fail");
                Debug.Log("Fail");
                testResult = false;
            }
            binaryReader.Close();
        }

        if (testResult) {
            Console.WriteLine("Smoke test: Success");
            Debug.Log("Success");
        } else {
            Console.WriteLine("Smoke test: Fail");
            Debug.Log("Fail");
        }
    }
}

class Perceptron {
    public static readonly int DISTRIBUTION_LAYER_SIZE = 5;
    private readonly int HIDDEN_LAYER_SIZE;
    private readonly int OUT_LAYER_SIZE;
    private readonly float LEARN_SPEED;
    private readonly float TRIGGERING_LEVEL;

    float[,] hiddenMatrix;
    float[,] outMatrix;
    float[] hiddenThresholds;
    float[] outThresholds;

    public Perceptron(int gensturesCount) : this(gensturesCount, 0.8f, DISTRIBUTION_LAYER_SIZE, 0.2f) { }

    public Perceptron(int gensturesCount, float triggeringLevel, int hiddenCount, float learnSpeed) {
        HIDDEN_LAYER_SIZE = DISTRIBUTION_LAYER_SIZE;
        OUT_LAYER_SIZE = gensturesCount;
        LEARN_SPEED = learnSpeed;

        hiddenMatrix = new float[HIDDEN_LAYER_SIZE, DISTRIBUTION_LAYER_SIZE];
        hiddenThresholds = new float[HIDDEN_LAYER_SIZE];

        outMatrix = new float[OUT_LAYER_SIZE, HIDDEN_LAYER_SIZE];
        outThresholds = new float[OUT_LAYER_SIZE];
    }

    public int recognize(float[] data) {
        return selectWinner(recognizeInternal(data)[1]);
    }

    public void teach(String[] dataFileNames, float strictness) {
        fillRandomWeights();
        BinaryReader[] binaryReaders = new BinaryReader[dataFileNames.Length];
        for (int i = 0; i < dataFileNames.Length; i++) {
            binaryReaders[i] = new BinaryReader(new FileStream(dataFileNames[i], FileMode.Open));
        }

        int epoch = 0;
        do {
            try {
                float accuracy = 0;
                for (int i = 0; i < dataFileNames.Length; i++) {
                    BinaryReader binaryReader = binaryReaders[i];
                    float[] dataRow = new float [] {
                        binaryReader.ReadSingle(),
                        binaryReader.ReadSingle(),
                        binaryReader.ReadSingle(),
                        binaryReader.ReadSingle(),
                        binaryReader.ReadSingle()
                    };
                    accuracy = teachSingle(dataRow, i);
                    Console.WriteLine("accuracy: " + accuracy);
                    Debug.Log("accuracy: " + accuracy);
                    if (accuracy < strictness) {
                        goto learningFinished;
                    }
                }
                epoch++;
            } catch (EndOfStreamException) {
                Console.WriteLine("unexpected end of file, finish learning");
                Debug.Log("unexpected end of file, finish learning");
                break;
            }             
        } while (true);

        learningFinished:
        for (int i = 0; i < dataFileNames.Length; i++) {
            binaryReaders[i].Close();
            Debug.Log("Learning Finished");
        }
    }

    public void saveToFile() {
        String fileName = "perceptron_" + DISTRIBUTION_LAYER_SIZE + "_" + HIDDEN_LAYER_SIZE + "_" + OUT_LAYER_SIZE + ".bin";
        BinaryWriter binaryWriter = new BinaryWriter(new FileStream(fileName, FileMode.Create));
        for (int i = 0; i < HIDDEN_LAYER_SIZE; i++) {
            for (int j = 0; j < DISTRIBUTION_LAYER_SIZE; j++) {
                binaryWriter.Write(hiddenMatrix[i, j]);
            }
        }
        for (int i = 0; i < HIDDEN_LAYER_SIZE; i++) {
            binaryWriter.Write(hiddenThresholds[i]);
        }

        for (int i = 0; i < OUT_LAYER_SIZE; i++) {
            for (int j = 0; j < HIDDEN_LAYER_SIZE; j++) {
                binaryWriter.Write(outMatrix[i, j]);
            }
        }
        for (int i = 0; i < OUT_LAYER_SIZE; i++) {
            binaryWriter.Write(outThresholds[i]);
        }
        binaryWriter.Close();
        Console.WriteLine("Perceptron state saved to: " + fileName);
        Debug.Log("Perceptron state saved to: " + fileName);
    }

    public void loadFromFile() {
        String fileName = "perceptron_" + DISTRIBUTION_LAYER_SIZE + "_" + HIDDEN_LAYER_SIZE + "_" + OUT_LAYER_SIZE + ".bin";
        BinaryReader binaryReader = new BinaryReader(new FileStream(fileName, FileMode.Open));
        for (int i = 0; i < HIDDEN_LAYER_SIZE; i++) {
            for (int j = 0; j < DISTRIBUTION_LAYER_SIZE; j++) {
                hiddenMatrix[i, j] = binaryReader.ReadSingle();
            }
        }
        for (int i = 0; i < HIDDEN_LAYER_SIZE; i++) {
            hiddenThresholds[i] = binaryReader.ReadSingle();
        }

        for (int i = 0; i < OUT_LAYER_SIZE; i++) {
            for (int j = 0; j < HIDDEN_LAYER_SIZE; j++) {
                outMatrix[i, j] = binaryReader.ReadSingle();
            }
        }
        for (int i = 0; i < OUT_LAYER_SIZE; i++) {
            outThresholds[i] = binaryReader.ReadSingle();
        }
        binaryReader.Close();
        Console.WriteLine("Perceptron state loaded from: " + fileName);
        Debug.Log("Perceptron state loaded from: " + fileName);
    }

    private void fillRandomWeights() {
        Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        Random rnd = new Random(unixTimestamp);

        for (int i = 0; i < HIDDEN_LAYER_SIZE; i++) {
            for (int j = 0; j < DISTRIBUTION_LAYER_SIZE; j++) {
                hiddenMatrix[i, j] = (float) (rnd.NextDouble() * 2 - 1);
            }
        }

        for (int i = 0; i < HIDDEN_LAYER_SIZE; i++) {
            hiddenThresholds[i] = (float) (rnd.NextDouble() * 2 - 1);
        }

        for (int i = 0; i < OUT_LAYER_SIZE; i++) {
            for (int j = 0; j < HIDDEN_LAYER_SIZE; j++) {
                outMatrix[i, j] = (float) (rnd.NextDouble() * 2 - 1);
            }
        }

        for (int i = 0; i < OUT_LAYER_SIZE; i++) {
            outThresholds[i] = (float) (rnd.NextDouble() * 2 - 1);
        }
    }

    /**
    * returns the worst error
    */
    private float teachSingle(float[] learnData, int expectedWinnerIndex) {
        if (learnData.Length != DISTRIBUTION_LAYER_SIZE 
            || expectedWinnerIndex < 0 || expectedWinnerIndex > OUT_LAYER_SIZE) {
            throw new Exception("Illegal arguments");
        }

        float[][] recognitionResult = recognizeInternal(learnData);

        float[] outErrors = new float[OUT_LAYER_SIZE];
        float worstError = 0;
        for (int i = 0; i < OUT_LAYER_SIZE; i++) {
            float expectedResult = 0f;
            if (i == expectedWinnerIndex) {
                expectedResult = 1f;
            }
            outErrors[i] = expectedResult - recognitionResult[1][i];

            if (worstError < Math.Abs(outErrors[i])) {
                worstError = Math.Abs(outErrors[i]);
            }
        }

        // hint: don't try to understand this
        float[] dfy = new float[HIDDEN_LAYER_SIZE];
        for (int i = 0; i < HIDDEN_LAYER_SIZE; i++) {
            dfy[i] = 0;
            for (int j = 0; j < OUT_LAYER_SIZE; j++) {
                dfy[i] = dfy[i] + recognitionResult[1][j] * (1 - recognitionResult[1][j]) *
                    outErrors[j] * outMatrix[j, i];
            }
        }

        // teach hidden layer matrix
        for (int i = 0; i < HIDDEN_LAYER_SIZE; i++) {
            for (int j = 0; j < DISTRIBUTION_LAYER_SIZE; j++) {
                hiddenMatrix[i, j] = hiddenMatrix[i, j] + LEARN_SPEED * learnData[j] *  dfy[i] * 
                    recognitionResult[0][i] * (1 - recognitionResult[0][i]);
                ;
            }
        }
        // teach hidden layer thresholds
        for (int i = 0; i < HIDDEN_LAYER_SIZE; i++) {
            hiddenThresholds[i] = hiddenThresholds[i] + 
                LEARN_SPEED * dfy[i] * 
                recognitionResult[0][i] * (1 - recognitionResult[0][i])
            ;
        }

        // teach out layer
        for (int i = 0; i < OUT_LAYER_SIZE; i++) {
            for (int j = 0; j < HIDDEN_LAYER_SIZE; j++) {
                outMatrix[i, j] = outMatrix[i, j] + 
                    LEARN_SPEED * outErrors[i] * recognitionResult[0][j] * 
                    recognitionResult[1][i] * (1 - recognitionResult[1][i])
                ;
            }
        }
        // teach out thresholds
        for (int i = 0; i < OUT_LAYER_SIZE; i++) {
            outThresholds[i] = outThresholds[i] + 
                LEARN_SPEED * outErrors[i] *
                recognitionResult[1][i] * (1 - recognitionResult[1][i]) 
            ;
        }

        return worstError;
    }

    /**
    * float[0] -- hidden layer results
    * float[1] -- out loayer results
    */
    private float[][] recognizeInternal(float[] data) {
        if (data.Length != DISTRIBUTION_LAYER_SIZE) {
            throw new Exception("Illegal arguments");
        }

        float[] hiddenResults = new float[HIDDEN_LAYER_SIZE];
        float[] outResults = new float[OUT_LAYER_SIZE];

        for (int i = 0; i < HIDDEN_LAYER_SIZE; i++) {
            float hiddenNeuronSum = hiddenThresholds[i];
            for (int j = 0; j < DISTRIBUTION_LAYER_SIZE; j++) {
                hiddenNeuronSum += hiddenMatrix[i, j] * data[j];
            }
            hiddenResults[i] = sigmoid(hiddenNeuronSum);
        }

        for (int i = 0; i < OUT_LAYER_SIZE; i++) {
            float outNeuronSum = outThresholds[i];
            for (int j = 0; j < HIDDEN_LAYER_SIZE; j++) {
                outNeuronSum += outMatrix[i, j] * hiddenResults[j];
            }
            outResults[i] = sigmoid(outNeuronSum);
        }

        return new float[][] {hiddenResults, outResults};
    }

    private int selectWinner(float[] data) {
        float winnerValue = data[0];
        int winnerIndex = 0;
        for (int i = 1; i < data.Length; i++) {
            if (data[i] > winnerValue) {
                winnerValue = data[i];
                winnerIndex = i;
            }
        }
        if (winnerValue > TRIGGERING_LEVEL) {
            return winnerIndex;
        } else {
            return -1;
        }
    }

    public static float sigmoid(float x) {
        return 1.0f / (1.0f + (float)Math.Exp(-x));
    }
}
