#include "TcpClient.h"
#include "Common.h"

#pragma comment(lib,"ws2_32.lib")
int Client::Init()
{
    WORD sockVersion = MAKEWORD(2, 2);
    WSADATA wsaData;
    return WSAStartup(sockVersion, &wsaData);
}
Client::Client(string ip, int port)
{
    memset(&server_addr, 0, sizeof(server_addr));
    server_addr.sin_family = AF_INET;
    if (inet_pton(AF_INET, ip.c_str(), &server_addr.sin_addr) == 0)
    {
        cout << "Server IP Address Error!";
        exit(1);
    }
    server_addr.sin_port = htons(port);
    server_addr_len = sizeof(server_addr);
    // create socket
    fd = socket(AF_INET, SOCK_STREAM, 0);
    if (fd < 0)
    {
        cout << "Create Socket Failed!";
        exit(1);
    }
}

Client::~Client()
{
    closesocket(fd);
}

void Client::Connect()
{
    cout << "Connecting......" << endl;
    if (connect(fd, (struct sockaddr*)&server_addr, server_addr_len) < 0)
    {
        cout << "Can not Connect to Server IP!";
        exit(1);
    }
    cout << "Connect to Server successfully." << endl;
}

void Client::Send(string str)
{
    PACKET_HEAD head;
    head.length = str.size() + 1;   // 注意这里需要+1
    int ret1 = send(fd, (const char*)&head, sizeof(head), 0);
    int ret2 = send(fd, str.c_str(), head.length, 0);
    if (ret1 < 0 || ret2 < 0)
    {
        cout << "Send Message Failed!";
        exit(1);
    }
}

string Client::Recv()
{
    PACKET_HEAD head;
    recv(fd, (char*)&head, sizeof(head), 0);

    char* buffer = new char[head.length];
    memset(buffer, 0, head.length);
    int total = 0;
    while (total < head.length)
    {
        int len = recv(fd, buffer + total, head.length - total, 0);
        if (len < 0)
        {
            cout << "recv() error!";
            break;
        }
        total = total + len;
    }
    string result(buffer);
    delete buffer;
    return result;
}

int mainxx()
{
    Client client("127.0.0.1", 15000);
    client.Connect();
    while (1)
    {
        string msg;
        getline(cin, msg);
        if (msg == "exit")
            break;
        client.Send(msg);
        cout << client.Recv() << endl;
    }
    return 0;
}