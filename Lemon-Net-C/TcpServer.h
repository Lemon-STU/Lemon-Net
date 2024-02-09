#pragma once
#include <WinSock2.h>

using namespace std;


class Server
{
private:
    struct sockaddr_in server_addr;
    int server_addr_len;
    int listen_fd;      // 监听的fd
    int max_fd;         // 最大的fd
    fd_set master_set;  // 所有fd集合，包括监听fd和客户端fd
    fd_set working_set; // 工作集合
    struct timeval timeout;

public:
    Server(int port);
    ~Server();
    int Init();
    void Bind();
    void Listen(int queue_len = 20);
    void Accept();
    void Run();
    void Recv(int nums);
};





