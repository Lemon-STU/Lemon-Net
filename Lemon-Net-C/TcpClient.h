#pragma once
#include <iostream>
#include<iostream>
#include<string>
#include<cstdlib>
#include<cstdio>
#include<cstring>
#include <WinSock2.h>
#include <ws2tcpip.h>
using namespace std;

class Client
{
private:
    struct sockaddr_in server_addr;
    int server_addr_len;
    int fd;
public:
    Client(string ip, int port);
    ~Client();
    int Init();
    void Connect();
    void Send(string str);
    string Recv();
};



