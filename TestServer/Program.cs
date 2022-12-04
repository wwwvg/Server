﻿using System.Net;
using System.Net.Sockets;

new Thread(RunServerAsync).Start(); // Run server method concurrently.
Thread.Sleep(500);
Console.ReadKey();

async void RunServerAsync()
{
    var listener = new TcpListener(IPAddress.Any, 55555);
    listener.Start();
    try
    {
        while (true)
            Accept(await listener.AcceptTcpClientAsync());
    }
    finally { listener.Stop(); }
}

async Task Accept(TcpClient client)
{
    await Task.Yield();
    try
    {
        using (client)
        using (NetworkStream n = client.GetStream())
        {
            byte[] data = new byte[255];
            await n.ReadAsync(data, 0, 255);

            int sum = Sum(data);
            if (sum == 0)
            {
                data[0] = 0;
                data[1] = 0;
                await n.WriteAsync(data, 0, 2);
            }
            else if (sum > 0)
            {
                var tuple = GetTwoBytes(sum);
                data[0] = tuple.Item1;
                data[1] = tuple.Item2;
                await n.WriteAsync(data, 0, 2);
            }
            else
            {
                n.Flush();
                client.Close();
            }
        }
    }
    catch (Exception ex) { Console.WriteLine(ex.Message); }
}

int Sum(byte[] data)
{
    int firstIndex = Array.IndexOf(data, (byte)10);
    int lastIndex = Array.IndexOf(data, (byte)11);

    if (firstIndex == -1 || lastIndex == -1 || firstIndex > lastIndex) // только мусор
        return -1;

    if(lastIndex - firstIndex == 1) // нет данных
        return 0;

    int sum = 0;
    for (int i = firstIndex + 1; i < lastIndex; i++) // подсчет
    {
        sum += data[i];
    }
    return sum;
}

(byte, byte) GetTwoBytes(int number) // максимальная сумма - два байта
{
    if(number <= 255)
        return ((byte)number, 0);

    byte lowByte = (byte)(number % 256);
    int remains = number - lowByte;
    byte highByte = (byte)(remains / 256);
    return (lowByte, highByte);
}

int FromTwoBytes(byte lowByte, byte highByte) // проверка
{
    return highByte * 256 + lowByte;
}