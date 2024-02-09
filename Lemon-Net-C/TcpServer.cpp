#include "TcpServer.h"
#include "Common.h"
#include <iostream>


using namespace std;
#pragma comment(lib,"ws2_32.lib")

int Server::Init()
{
    WORD sockVersion = MAKEWORD(2, 2);
    WSADATA wsaData;
    
    return WSAStartup(sockVersion, &wsaData);
}

Server::Server(int port)
{
    memset(&server_addr,0, sizeof(server_addr));
    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = htons(INADDR_ANY);
    server_addr.sin_port = htons(port);
    // create socket to listen
    listen_fd = socket(PF_INET, SOCK_STREAM, 0);
    if (listen_fd < 0)
    {
        cout << "Create Socket Failed!";
        exit(1);
    }
    int opt = 1;
    // 允许重用本地地址和端口
    setsockopt(listen_fd, SOL_SOCKET, SO_REUSEADDR, (const char*)&opt, sizeof(opt));
}

Server::~Server()
{
    for (int fd = 0; fd <= max_fd; ++fd)
    {
        if (FD_ISSET(fd, &master_set))
        {
            closesocket(fd);
        }
    }
}

void Server::Bind()
{
    if (-1 == (bind(listen_fd, (struct sockaddr*)&server_addr, sizeof(server_addr))))
    {
        cout << "Server Bind Failed!";
        exit(1);
    }
    cout << "Bind Successfully.\n";
}

void Server::Listen(int queue_len)
{
    if (-1 == listen(listen_fd, queue_len))
    {
        cout << "Server Listen Failed!";
        exit(1);
    }
    cout << "Listen Successfully.\n";
}

void Server::Accept()
{
    struct sockaddr_in client_addr;
    int client_addr_len = sizeof(client_addr);

    int new_fd = accept(listen_fd, (struct sockaddr*)&client_addr, &client_addr_len);
    if (new_fd < 0)
    {
        cout << "Server Accept Failed!";
        exit(1);
    }

    cout << "new connection was accepted.\n";
    // 将新建立的连接的fd加入master_set
    FD_SET(new_fd, &master_set);
    if (new_fd > max_fd)
    {
        max_fd = new_fd;
    }
}

void Server::Run()
{
    max_fd = listen_fd; // 初始化max_fd
    FD_ZERO(&master_set);
    FD_SET(listen_fd, &master_set); // 添加监听fd

    while (1)
    {
        FD_ZERO(&working_set);
        memcpy(&working_set, &master_set, sizeof(master_set));

        timeout.tv_sec = 30;
        timeout.tv_usec = 0;

        int nums = select(max_fd + 1, &working_set, NULL, NULL, &timeout);
        if (nums < 0)
        {
            cout << "select() error!";
            exit(1);
        }

        if (nums == 0)
        {
            //cout << "select() is timeout!";
            continue;
        }

        if (FD_ISSET(listen_fd, &working_set))
            Accept(); // 有新的客户端请求
        else
            Recv(nums); // 接收客户端的消息
    }
}

void Server::Recv(int nums)
{
    for (int fd = 0; fd <= max_fd; ++fd)
    {
        if (FD_ISSET(fd, &working_set))
        {
            bool close_conn = false; // 标记当前连接是否断开了

            PACKET_HEAD head;
            recv(fd, (char*) & head, sizeof(head), 0); // 先接受包头，即数据总长度
            // std::cout << head.length << std::endl;
            char* buffer = new char[head.length];
            memset(buffer,0, head.length);
            int total = 0;
            while (total < head.length)
            {
                int len = recv(fd, buffer + total, head.length - total, 0);
                if (len < 0)
                {
                    cout << "recv() error!";
                    close_conn = true;
                    break;
                }
                total = total + len;
            }

            if (total == head.length) // 将收到的消息原样发回给客户端
            {
                int ret1 = send(fd, (const char*) & head, sizeof(head), 0);
                int ret2 = send(fd, buffer, head.length, 0);
                if (ret1 < 0 || ret2 < 0)
                {
                    cout << "send() error!";
                    close_conn = true;
                }
            }

            delete buffer;

            if (close_conn) // 当前这个连接有问题，关闭它
            {
                closesocket(fd);
                FD_CLR(fd, &master_set);
                if (fd == max_fd) // 需要更新max_fd;
                {
                    while (FD_ISSET(max_fd, &master_set) == false)
                        --max_fd;
                }
            }
        }
    }
}

int mainxx2()
{
    Server server(15000);
    server.Bind();
    server.Listen();
    server.Run();
    return 0;
}