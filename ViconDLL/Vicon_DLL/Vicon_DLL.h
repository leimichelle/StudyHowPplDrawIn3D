﻿#pragma once
//#include <functional>
//#include <chrono>
//#include <future>
//#include <cstdio>

#include <DataStreamClient.h>

struct PenData
{
	float x, y, z;
	float qx, qy, qz, qw;
	int valid;
};

extern "C" __declspec(dllexport) int StartVicon();
extern "C" __declspec(dllexport) PenData FetchNewViconFrame();


//class CallBackTimer
//{
//public:
//	CallBackTimer()
//		:_execute(false)
//	{}
//
//	~CallBackTimer() {
//		if (_execute.load(std::memory_order_acquire)) {
//			stop();
//		};
//	}
//
//	void stop()
//	{
//		_execute.store(false, std::memory_order_release);
//		if (_thd.joinable())
//			_thd.join();
//	}
//
//	void start(int interval, std::function<void(void)> func)
//	{
//		if (_execute.load(std::memory_order_acquire)) {
//			stop();
//		};
//		_execute.store(true, std::memory_order_release);
//		_thd = std::thread([this, interval, func]()
//		{
//			while (_execute.load(std::memory_order_acquire)) {
//				func();
//				std::this_thread::sleep_for(
//					std::chrono::milliseconds(interval));
//			}
//		});
//	}
//
//	bool is_running() const noexcept {
//		return (_execute.load(std::memory_order_acquire) &&
//			_thd.joinable());
//	}
//
//private:
//	std::atomic<bool> _execute;
//	std::thread _thd;
//};