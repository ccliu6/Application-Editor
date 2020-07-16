using System;
using System.Text;

namespace EncrytionCSharp
{
    public enum eCryptoStatus
    {
        eCryptoStatusOK,
        eMissingKey,
        eKeyMismatch
    }

    public class RyptoObject
    {
        string m_key;
        Random randNumGenerator;
        Encoding encoder = Encoding.UTF8;

        public RyptoObject(string name, bool bAddRandomness = false)
        {
            SeedGenerator(name, bAddRandomness);
        }

        ~RyptoObject()
        {

        }

        // SetKey() sets a unique key.
        // WARNING: key should be alphanumeric or else
        // the ciphertext may be invalid
        public void SetKey(ref string key)
        {
            m_key = key;
        }

        public void SeedGenerator(string name, bool bAddRandomness = false)
        {
            int seed = 169172;
            if (name.Length > 0)
            {
                int i = 0;
                for (int j = 0; j < name.Length; j++)
                {
                    if (Char.IsDigit(name, j))
                    {
                        i = j;
                        break;
                    }
                }
                byte unique_c = (byte)name[i];
                if (unique_c != 0)
                {
                    seed = 13517 * unique_c;
                    if (bAddRandomness)
                    {
                        seed = (seed % 1000) * 11 * unique_c;
                    }
                }
            }
            randNumGenerator = new Random(seed);
        }

        // encrypt: encrypt the input str if we have a key.
        // Returns eMissingKey if we dont have a key.
        // or eCryptoStatusOk when successful.
        // WARNING: input string should not contain special characters
        // (':', "\r\n" and ' ' are ok)
        public eCryptoStatus encrypt(string inputStr, out byte[] sValue)
        {
            int keyLength = m_key.Length;
            if (keyLength == 0)
            {
                sValue = null;
                return eCryptoStatus.eMissingKey;
            }

            string inputStrCpy = inputStr.Replace("\r\n", ";").Replace(' ', '`');
            byte[] sBuffer = new byte[keyLength + inputStrCpy.Length];

            doEncrypt(encoder.GetBytes(m_key), sBuffer, 0, false);
            doEncrypt(encoder.GetBytes(inputStrCpy), sBuffer, keyLength, true);

            sValue = sBuffer;
            return eCryptoStatus.eCryptoStatusOK;
        }

        // doEncrypt
        // pStr is the source string
        // sBuffer is an output buffer for the encrypted string
        private void doEncrypt(byte[] pStr, byte[] sBuffer, int nStart, bool bRecycle)
        {
            byte[] dumBuffer = null;
            int r = 0;
            int indexI = 0;
            int indexO = nStart;
            while (indexI < pStr.Length)
            {
                if (!bRecycle || r == 0)
                {
                    r = randNumGenerator.Next();
                    dumBuffer = encoder.GetBytes(r.ToString()); // convert string to byte array
                }
                int randLen = dumBuffer.Length;
                //for all the chars in pStr (source string) with index less than randLen
                int dumBufIndex = 0;
                while (dumBufIndex < randLen && indexI < pStr.Length)
                {
                    byte c1 = dumBuffer[dumBufIndex];
                    byte c = pStr[indexI];
                    byte c2;
                    if (c >= 42 && c <= 46)
                    {
                        c2 = (byte)(c + 76);
                    }
                    else
                    {
                        c2 = (byte)(c - (c1 - 41));
                        if (c2 == 94)
                            c2 = (byte)124;
                        else if (c2 == 92)
                            c2 = (byte)123;
                        else if (c2 == 60)
                            c2 = (byte)116;
                        else if (c2 == 38)
                            c2 = (byte)117;

                    }

                    sBuffer[indexO++] = c2;
                    indexI++;
                    dumBufIndex++;
                }
            }
        }

        // sValue is the input encrypted string
        // Returns eCryptoStatus {OK or missing key or key-mismatch} code.
        public eCryptoStatus decrypt(string sValue, ref string outputStr)
        {
            if (m_key.Length == 0)
                return eCryptoStatus.eMissingKey;

            var sbytes = encoder.GetBytes(sValue);
            int sBufferSize = sbytes.Length;
            byte[] sBuffer = new byte[sBufferSize+1];
            int keyLen = m_key.Length;

            int outIndex = 0;
            byte[] dumBuffer = null;
            bool bProcessKey = true;
            int index = 0;
            while (index < sbytes.Length)
            {
                if (bProcessKey)
                {
                    int r = randNumGenerator.Next();
                    dumBuffer = encoder.GetBytes(r.ToString());
                    if (keyLen == 0)
                    {
                        bProcessKey = false;
                    }
                }
                int randLen = dumBuffer.Length;

                int dumBufIndex = 0;
                while (dumBufIndex < randLen && index < sbytes.Length && sbytes[index] != 0)
                {
                    byte c1 = dumBuffer[dumBufIndex];
                    byte c2 = sbytes[index];
                    byte c;
                    if (c2 >= 118 && c2 <= 122)
                    {
                        c = (byte)(c2 - 76);
                    }
                    else
                    {
                        if (c2 == 124)
                            c2 = (byte)94;
                        else if (c2 == 123)
                            c2 = (byte)92;
                        else if (c2 == 116)
                            c2 = (byte)60;
                        else if (c2 == 117)
                            c2 = (byte)38;
                        c = (byte)(c2 + (c1 - 41));
                    }

                    sBuffer[outIndex++] = c;
                    index++;
                    dumBufIndex++;
                    

                    if (keyLen > 0)
                    {
                        keyLen--;
                        if (keyLen == 0)
                        {
                            var sKey = encoder.GetString(sBuffer);
                            if (sKey.Contains(m_key))
                            {
                                Array.Clear(sBuffer, 0, sBufferSize);
                                outIndex = 0;
                                break;
                            }
                            else
                            {
                                return eCryptoStatus.eKeyMismatch;
                            }
                        }
                    }
                }
            }

            if (keyLen > 0)
                return eCryptoStatus.eKeyMismatch;

            // The output string is returned only if key has been matched successfully.
            outputStr = encoder.GetString(sBuffer).Replace(";", "\r\n").Replace("`", " ");

            return eCryptoStatus.eCryptoStatusOK;
        }
    }
}
