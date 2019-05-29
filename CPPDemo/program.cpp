#include <iostream>
#include <climits>
#include <string>
#include <cfloat>
#include <cstdio>
#include <cctype>
#include <iomanip>  
#include <iostream>  
#include <sstream> 
#include "scoped_ptr.h"
using namespace std;


int main(int argc, char** argv) {
	char line[4];
	cin.getline(line, 4);
	if (cin.eof())
	{
		cout << "fuck eof";
	}
	if (cin.fail())
	{
		cout << "fuck fail";
	}
	if (cin.bad())
	{
		cout << "fuck bad";
	}

	return 0;
}