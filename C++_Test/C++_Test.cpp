// C++_Test.cpp: 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <complex>
#include <iostream>
#include <cmath>

#define PI 3.141592653589793

using namespace std;

bool FFT(complex<double> * TD, complex<double> * FD, int r)
{
	//一维快速Fourier变换。
	//complex<double> * TD ——指向时域数组的指针; complex<double> * FD ——指向频域数组的指针; r ——2的幂数，即迭代次数
	long count; // Fourier变换点数//以前是LONG
	int i, j, k; // 循环变量
	int bfsize, p; // 中间变量
	double angle; // 角度
	complex<double> *W, *X1, *X2, *X;
	count = 1 << r; // 计算Fourier变换点数为1左移r位
	W = new complex<double>[count / 2];
	X1 = new complex<double>[count];
	X2 = new complex<double>[count]; // 分配运算所需存储器
									 // 计算加权系数（旋转因子w的i次幂表）
	for (i = 0; i < count / 2; i++) {
		angle = -i *  PI * 2 / count;
		W[i] = complex<double>(cos(angle), sin(angle));
	}
	// 将时域点写入X1
	memcpy(X1, TD, sizeof(complex<double>) * count);
	// 采用蝶形算法进行快速Fourier变换
	for (k = 0; k < r; k++) {
		for (j = 0; j < 1 << k; j++) {
			bfsize = 1 << (r - k);
			for (i = 0; i < bfsize / 2; i++) {
				p = j * bfsize;
				X2[i + p] = X1[i + p] + X1[i + p + bfsize / 2] * W[i * (1 << k)];
				X2[i + p + bfsize / 2] = X1[i + p] - X1[i + p + bfsize / 2] * W[i * (1 << k)];
			}
		}
		X = X1;
		X1 = X2;
		X2 = X;
	}

	// 重新排序
	for (j = 0; j < count; j++) {
		p = 0;
		for (i = 0; i < r; i++) {
			if (j&(1 << i)) {
				p += 1 << (r - i - 1);
			}
		}
		FD[j] = X1[p];
	}
	// 释放内存
	delete W;
	delete X1;
	delete X2;
	return true;
}
double Test1(double num,int N)
{
	return 0.54 - 0.46*(cos(2 * 3.1415*num / N));
}

int main()
{
	//complex<double> x(1, 2.1), y;

	//cout << "x = " << x << endl;

	//y = x + 1.0;
	//
	//cout << "\ny = " << y << endl;

	//// if (x < y) not allowed - no standard definition

	//// cout << "x less than y" << endl;
	int nums[]= { 1, 3, 2, 5, 4, 6, 8, 4, 5, 2, 1 };
	complex<double> * args = new complex<double>[16];
	complex<double> * argsOut = new complex<double>[16];
	for (int i = 0; i < 11; i++)
	{
		args[i].real(nums[i]);
	}
	FFT(args, argsOut, 5);
	for (int i = 0; i < 11; i++)
	{
		cout << argsOut[i].real() << " " << argsOut[i].imag() << endl;
	}
	int a;
	cin >> a;
    return 0;
}

