using System.Xml.Schema;

namespace webApiWares
{
    public class OrderWareInfoExt
    {
        public int IdWare; // Id ������ � �������� �������

        public string Name = string.Empty; //������������ ������
        public int Count { get; set; } //���������� � ������
        public decimal Price { get; set; } //���� �� 1 ������� ������

        public int Total; // Count * Price
    }

    record class WareInfo(string Name, int Count, decimal Price);

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddSingleton<DataContext>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            //��� �������� ����� ������ ���� ������ � ��������� ����� DataContext
            //�������� ������ ����������� � ��� ������������.
            //��������������� ��� ������  Singleton, �.�. ������ ���������� �� ���������� ������ �������
            //��� ��������� ��������� (�-�� ������ ��) ���� ���� Orders � Wares
            //����� � �������� ����� �������� ������ ������ DataContext ������������ �����:
            //  app.Services.GetService<DataContext>();

            //������� �� ASP NET https://metanit.com/sharp/aspnet6/1.1.php
            //������� � ����������� - ��������� LINQ  https://metanit.com/sharp/tutorial/15.1.php
            //������ �������� ��� List<T>

            var orders = app.Services.GetService<DataContext>()?.Orders;
            var wares = app.Services.GetService<DataContext>()?.Wares;

            app.MapGet("/Wares", () =>
            {
                var dc = app.Services.GetService<DataContext>();
                return Results.Ok(dc.Wares); //��� ��������� ���������  Results API https://metanit.com/sharp/aspnet6/10.1.php
            });

            app.MapPost("/Wares/add", (string ware) =>
            {
                //����� ������ ��������� ����� ����� � ����� ������ �������
                //return Results.Ok();

                wares.Add(ware);
            });
            app.MapDelete("/Wares/delete/{id}", (int id) =>
            {
                //����� ������ ������� ����� �� id

                //return Results.Ok();

                wares.RemoveAt(id--);
            });

            app.MapGet("/Orders", () =>
            {
                //����� ������ ������� ������ ������� � ����:
                //id, date, employee, countWares, ��� countWares - ���������� ����������� � ������ �������

                //return Results.Ok();

                return orders;
            });

            app.MapPost("/Orders/add", () =>
            {
                //����� ��������� �������� ����� ����� - ������ �����, ������ ��������� ������ api
                //id - ������ ����������� ������������� (��� ����� ����������� ���� � ��): ������������ ������������ + 1

                Order order = new()
                {
                    Id = orders.Count + 1,

                    Date = DateTime.Now
                };

                orders.Add(order);
            });

            app.MapPost("/Orders/addWare/{idOrder}", (int idOrder, int idWare, int count, decimal price) =>
            {
                //����� ��������� ���������� �� ������ � �����
                //�� ��� �������� "�� �������", ��� ��������, �������� ����������
                //���������� "���������" ��� (�� CS8602 ������������� �������� ������ ������ - ����� �� �������� ��������)

                var order = orders.FirstOrDefault(order => order.Id == idOrder);
                order.Wares.Add(new OrderWareInfo { Name = wares.FirstOrDefault(x => wares.IndexOf(x) == idWare), Count = count, Price = price });
                return Results.Ok(order);
            });

            app.MapPost("/Orders/addRangeWares/{idOrder}", (int idOrder, WareInfo[] _waresinfo) =>
            {
                //����� ��������� ���������� �� ������ � �����, �� ����� ��������
                //���� record ����� wareInfo - ���������� ������� ��� ���������, ������� ����� ��������� ��� frontend
                //��� ����, ����� �� ����� �������� ����� ��������� ������� � �����

                OrderWareInfo orderWareInfo = new();

                foreach (WareInfo wareInfo in _waresinfo)
                {
                    orderWareInfo.Name = wareInfo.Name;
                    orderWareInfo.Count = wareInfo.Count;
                    orderWareInfo.Price = wareInfo.Price;

                    orders[idOrder].Wares.Add(orderWareInfo);
                }
            });
            
            app.MapGet("/Orders/getWares/{idOrder}", (int idOrder) =>
            {
                //����� ������ ������� ������ ������� ������ � ����:
                //idWare, name (������������ ������), count, price, total, ��� total - count*price

                List<OrderWareInfoExt> orderWareInfoExt = new();

                for (int i = 0; i < orders[idOrder].Wares.Count; i++)
                {
                    orderWareInfoExt[i].IdWare = wares.IndexOf(orders[idOrder].Wares[i].Name);
                    orderWareInfoExt[i].Name = orders[idOrder].Wares[i].Name;
                    orderWareInfoExt[i].Count = orders[idOrder].Wares[i].Count;
                    orderWareInfoExt[i].Price = orders[idOrder].Wares[i].Price;
                    orderWareInfoExt[i].Total = orders[idOrder].Wares[i].Count * (int)orders[idOrder].Wares[i].Price;
                }

                return orderWareInfoExt;
            });

            //��������� �������, ����� ��� ���� ������ �� ��������� ����������
            app.MapPost("/JanuaryDays", (string firstDay) => 
            {
                /*������������� ������ ����� 1�31 ������������ (����� ������).
                ������� ��� ������� �� ����� ������ �[�����] ������, [���� ������]�. 
                ���� ������ 1 ������ ������ ���������� �������� ���������� firstDay, ��������� ������
                ��������� �������� ��� ������ ��� ������, � �������� ���������� �����. 
                
                �������� �������� - ������ - �������� "�������"

                ��������� 1: ��� ���� ������ ����� ������� ������ � ���������� ����, ����� ���������� � ���� �� �������.
                ��������� 2: ������ ��� ������ ����� ��������� � ������� �������� ������� �� �������.

                �������� ����������:
                ��� ������ ���������� ��� ������ 1 ������ ��� ��� ���������
                ���������. ��������, ��� ��������:
                1 ������, �������
                2 ������, �����
                3 ������, �������
                4 ������, �������
                5 ������, �������
                6 ������, �����������
                7 ������, �����������
                � �.�.*/

                string[] days = new string[] { "�����������", "�������", "�����", "�������", "�������", "�������", "�����������" };

                int dayId = Array.IndexOf(days, firstDay);

                for (int i = 0; i < 31; i++)
                {
                    Console.WriteLine($"{i + 1} ������, {days[dayId]}");

                    dayId++;

                    if (dayId > 6) dayId = 0;
                }
            });

            app.Run();
        }
    }
}