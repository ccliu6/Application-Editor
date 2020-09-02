// Encryption.cpp : This file contains the 'main' function. Program execution begins and ends there.
// It is used to test Bruker's CryptoObject C++ <=> C#
//

#include "pch.h"

#include <iostream>
#include <fstream>

#include "ryptoobject.h"

int main()
{

	for (int i = 0; i < 2; i++)
	{
		bool bEncrypt = i == 0;

		std::ifstream is(bEncrypt ? "C:\\Spm1\\Application Modes\\APi.txt" : "C:\\Spm1\\Application Modes\\APe.bin", std::ifstream::binary);
		if (is)
		{
			// get length of file:
			is.seekg(0, is.end);
			int length = (int)is.tellg();
			is.seekg(0, is.beg);

			char * buffer = new char[length+1];
			memset(buffer, 0, length + 1);

			std::cout << "Reading " << length << " characters... \n";
			// read data as a block:
			is.read(buffer, length);
			if (is)
				std::cout << "all characters read successfully.\n";
			else
				std::cout << "error: only " << is.gcount() << " could be read\n";
			is.close();


			CryptoObject encrypter("");
			encrypter.SetKey("a4rR1lKeIHmGuWj5jm5TX1aiAWdRFWkFRAkGJSK");
			std::string cipherStr;
			if (bEncrypt)
				encrypter.encrypt(buffer, cipherStr);
			else
				encrypter.decrypt(buffer, cipherStr);

			delete[] buffer;

			// Write out
			std::ofstream outfile(bEncrypt ? "C:\\Spm1\\Application Modes\\APe.bin" : "C:\\Spm1\\Application Modes\\APo.txt", std::ifstream::binary);
			outfile.write(cipherStr.c_str(), cipherStr.length());
			outfile.close();

			std::cout << "Writing " << cipherStr.length() << " characters... \n";;
		}
	}
}
