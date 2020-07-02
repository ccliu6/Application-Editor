// Bruker Confidential  
// Copyright Bruker Corporation 2009-2012. All rights reserved.
//

#pragma once
//  CryptoObject provides an encryption method that generates text in the readable ascii range that can be embedded in an xml document.
// 
// The encryption scheme should generate characters that will not break the xml readability using a simple IE browser.
// The encryption scheme is using a random-number generator

//
#include <string>
	enum eCryptoStatus { eCryptoStatusOK, eMissingKey, eKeyMismatch};

class CryptoObject
{
private:
	char m_key[40];
public:
	CryptoObject(const std::string& name, bool bAddRandomness=false);
	virtual ~CryptoObject(void);
	// sValue is the encrypted string
	eCryptoStatus encrypt(const std::string &inputStr, std::string &sValue);
	eCryptoStatus decrypt(const std::string &sValue, std::string &outputStr);	// outputStr is the result decrypted string 
	void SetKey(const char *key);
private:
	void doEncrypt(const char *pStr, char *sBuffer, bool bRecycle);
	void SeedGenerator(const std::string& name, bool bAddRandomness);

};
