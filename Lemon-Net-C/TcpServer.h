#pragma once
#include <WinSock2.h>

using namespace std;


class Server
{
private:
    struct sockaddr_in server_addr;
    int server_addr_len;
    int listen_fd;      // ������fd
    int max_fd;         // ����fd
    fd_set master_set;  // ����fd���ϣ���������fd�Ϳͻ���fd
    fd_set working_set; // ��������
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





