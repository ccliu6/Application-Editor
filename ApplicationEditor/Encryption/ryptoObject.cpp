// Bruker Confidential  
// Copyright Bruker Corporation 2009-2014. All rights reserved.
//
#include "pch.h"
#include ".\ryptoobject.h"
//#include "Boost\algorithm\string\replace.hpp"
#include <algorithm>

using namespace std;
//CryptoObject::CryptoObject()
//{
//	srand(169172);
//}

CryptoObject::CryptoObject(const string& name, bool bAddRandomness)
{
	memset(m_key,'\0',40);
	SeedGenerator(name, bAddRandomness);
}

CryptoObject::~CryptoObject(void)
{
}

// SetKey() sets a unique key.
// WARNING: key should be alphanumeric or else
// the ciphertext may be invalid
void CryptoObject::SetKey(const char *key)
{
	strncpy_s(m_key, key, 39);
}

// seedGenerator:  seed generator
void CryptoObject::SeedGenerator(const string &name, bool bAddRandomness)
{
	int seed = 169172;		// allow a default seed
	if (name.length() > 0)
	{
		rsize_t i = 0;
		for (rsize_t j=0; j< name.length(); j++)
		{
			if (isdigit(name[j]))
			{
				i = j;
				break;
			}
		}
		char unique_c = name[i];
		if (unique_c != 0)
		{
			seed = 13517 * unique_c;
			if (bAddRandomness)
			{
				seed = (seed%1000) * 11*unique_c;
			}
		}
	}
	srand(seed);
}


// encrypt: encrypt the input str if we have a key.
// Returns eMissingKey if we dont have a key.
// or eCryptoStatusOk when successful.
// WARNING: input string should not contain special characters
// (':', "\r\n" and ' ' are ok)
eCryptoStatus CryptoObject::encrypt(const std::string &inputStr, std::string &sValue)
{
	if (m_key[0] == '\0')
		return eMissingKey;
	std::string inputStrCpy = inputStr;
	std::replace(inputStrCpy.begin(), inputStrCpy.end(), ' ', '`');
	//boost::algorithm::replace_all(inputStrCpy, "\r\n", ";");
	//boost::algorithm::replace_all(inputStrCpy, " ", "`");
	const char *pStr = inputStrCpy.c_str();
	size_t sBufferSize = std::max(inputStrCpy.size(), strlen(m_key)) + 1;
	char* sBuffer = new char[sBufferSize];
	memset(sBuffer,'\0', sBufferSize);
	doEncrypt(m_key, sBuffer,false);	// do not recycle when encrypting the key.
	sValue = sBuffer;

	memset(sBuffer,'\0', sBufferSize);
	doEncrypt(pStr,sBuffer, true);
	sValue.append(sBuffer);

	delete[] sBuffer;

	return eCryptoStatusOK;
}

// doEncrypt
// pStr is the source string
// sBuffer is an output buffer for the encrypted string
// 
void CryptoObject::doEncrypt(const char *pStr, char *sBuffer, bool bRecycle)
{
	char *pOutStr = sBuffer;
	char dumBuffer[20];
	int r=0;
	while (*pStr)
	{
		if (!bRecycle || r==0)
		{
			r = rand();
			sprintf_s(dumBuffer,"%d",r);
		}

		size_t randLen = strlen(dumBuffer);
		for (size_t i=0; i< randLen && *pStr; i++)
		{
			char c1 = dumBuffer[i];
			char c = *pStr++;
			char c2;
			if (c>= 42 && c<=46)
			{
				c2 = c + 76;
			}
			else
			{
				c2 = c - (c1-41);
				if (c2 == 94)
					c2 = 124;
				else if (c2 == 92)
					c2 = 123;
				else if (c2 == 60)	// xml special character
					c2 = 116;
				else if (c2 == 38)	// xm special character
					c2 = 117;

			}
			*pOutStr = c2;
			pOutStr++;
		}
	}

}

// sValue is the input encrypted string
// Returns eCryptoStatus {OK or missing key or key-mismatch} code.
eCryptoStatus CryptoObject::decrypt(const std::string &sValue, std::string &outputStr)
{
	if (m_key[0] == '\0')
		return eMissingKey;
	const char *pStr = sValue.c_str();
	size_t sBufferSize = sValue.size() + 1;
	char* sBuffer = new char[sBufferSize];
	memset(sBuffer,'\0', sBufferSize);
	///string sKeepBuffer;
	//eCryptoStatus cryptoStatus = doDecrypt(sValue, sBuffer);
	size_t keyLen = strlen(m_key);

	//int bufIndex=0;
	int outIndex=0;

	char dumBuffer[20];
	bool bProcessKey = true;
	while (*pStr)
	{
		if (bProcessKey)
		{	// This will recycle the dumBuffer for the encrypted value (only).
			int r = rand();
			sprintf_s(dumBuffer,"%d",r);
			if (keyLen ==0)
			{
				bProcessKey = false;
			}
		}
		size_t randLen = strlen(dumBuffer);
		for (size_t i=0; i< randLen && *pStr; i++)
		{
			char c1 = dumBuffer[i];
			char c2 = *pStr++;
			char c;
			if (c2 >=118 && c2 <=122)
			{
				c = c2 -76;
			}
			else
			{
				if (c2 == 124)
					c2 = 94;
				else if (c2 == 123)
					c2 = 92;
				else if (c2 == 116)
					c2 = 60;
				else if (c2 == 117)
					c2 = 38;
				c = c2 + (c1-41);
			}
			sBuffer[outIndex++] = c;
			if (keyLen >0)
			{
				keyLen--;
				if (keyLen ==0)
				{
					if (strcmp(sBuffer, m_key) != 0)
					{
						return eKeyMismatch;
					}
					else
					{
						// reset the string buffer
						memset(sBuffer,'\0', sBufferSize);
						outIndex =0;
						// call rand
						break;	// the for loop to the outmost loop where rand is called
					}
				}
			}
		}
	}
	if (keyLen >0)
		return eKeyMismatch;
	// The output string is returned only if key has been matched successfully.
	outputStr = sBuffer;

	//boost::replace_all(outputStr, ";", "\r\n");
	//boost::replace_all(outputStr, "`", " ");
	std::replace(outputStr.begin(), outputStr.end(), '`', ' ');
	delete[] sBuffer;

	return eCryptoStatusOK;
}
