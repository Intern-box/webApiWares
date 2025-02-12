namespace webApiWares
{
    public class OrderWareInfoExt
    {
        public int IdWare; // Id товара в каталоге товаров
        public string Name = string.Empty; //Наименование товара
        public int Count { get; set; } //Количество в заказе
        public decimal Price { get; set; } //Цена за 1 единицу товара
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

            //Для простоты вынес псевдо базу данных в отдельный класс DataContext
            //Тестовые данные заполняются в его конструкторе.
            //Зарегистрирован как сервис  Singleton, т.е. объект существует на протяжении работы сервиса
            //Для получения коллекций (а-ля таблиц БД) есть поля Orders и Wares
            //Чтобы в конечной точке получить объект класса DataContext используется метод:
            //  app.Services.GetService<DataContext>();

            //справка по ASP NET https://metanit.com/sharp/aspnet6/1.1.php
            //выборки и объединения - используй LINQ  https://metanit.com/sharp/tutorial/15.1.php
            //данные хранятся как List<T>

            var orders = app.Services.GetService<DataContext>()?.Orders;
            var wares = app.Services.GetService<DataContext>()?.Wares;

            app.MapGet("/Wares", () =>
            {
                try { return Results.Ok(app.Services.GetService<DataContext>().Wares); } //для упрощения используй Results API https://metanit.com/sharp/aspnet6/10.1.php

                catch (Exception) { return Results.Ok(string.Empty); }
            });

            app.MapPost("/Wares/add", (string ware) =>
            {
                //метод должен добавлять новый товар в общий список товаров
                //return Results.Ok();

                wares.Add(ware);
            });
            app.MapDelete("/Wares/delete/{id}", (int id) =>
            {
                //метод должен удалять товар по id

                //return Results.Ok();

                if (wares.Count < id) wares.RemoveAt(id--);
            });

            app.MapGet("/Orders", () =>
            {
                //метод должен вернуть список заказов в виде:
                //id, date, employee, countWares, где countWares - количество привязанных к заказу товаров

                //return Results.Ok();

                return orders;
            });

            app.MapPost("/Orders/add", () =>
            {
                //метод позволяет добавить новый заказ - только шапку, товары добавляем другим api
                //id - должен расчитаться автоматически (как будто вычисляемое поле в БД): максимальный существующий + 1

                Order order = new()
                {
                    Id = orders.Count + 1,
                    Date = DateTime.Now
                };

                orders.Add(order);
            });

            app.MapPost("/Orders/addWare/{idOrder}", (int idOrder, int idWare, int count, decimal price) =>
            {
                //метод добавляет информацию по товару в заказ
                //но его написали "на коленке", нет проверок, возможны исключения
                //необходимо "причесать" код (на CS8602 Разыменование вероятно пустой ссылки - можно не обращать внимания)

                var order = orders.FirstOrDefault(order => order.Id == idOrder);
                order.Wares.Add(new OrderWareInfo { Name = wares.FirstOrDefault(x => wares.IndexOf(x) == idWare), Count = count, Price = price });
                return Results.Ok(order);
            });

            app.MapPost("/Orders/addRangeWares/{idOrder}", (int idOrder, WareInfo[] _waresinfo) =>
            {
                //метод добавляет информацию по товару в заказ, но сразу массивом
                //есть record класс wareInfo - необходимо указать его структуру, которую будет присылать нам frontend
                //для того, чтобы мы могли добавить сразу несколько товаров в заказ

                OrderWareInfo orderWareInfo = new();

                try
                {
                    foreach (WareInfo wareInfo in _waresinfo)
                    {
                        orderWareInfo.Name = wareInfo.Name;
                        orderWareInfo.Count = wareInfo.Count;
                        orderWareInfo.Price = wareInfo.Price;
                        orders[idOrder].Wares.Add(orderWareInfo);
                    }
                }
                catch (Exception) { }
            });
            
            app.MapGet("/Orders/getWares/{idOrder}", (int idOrder) =>
            {
                //метод должен вернуть список товаров заказа в виде:
                //idWare, name (наименование товара), count, price, total, где total - count*price

                List<OrderWareInfoExt> orderWareInfoExt = new();

                try
                {
                    for (int i = 0; i < orders[idOrder].Wares.Count; i++)
                    {
                        orderWareInfoExt[i].IdWare = wares.IndexOf(orders[idOrder].Wares[i].Name);
                        orderWareInfoExt[i].Name = orders[idOrder].Wares[i].Name;
                        orderWareInfoExt[i].Count = orders[idOrder].Wares[i].Count;
                        orderWareInfoExt[i].Price = orders[idOrder].Wares[i].Price;
                        orderWareInfoExt[i].Total = orders[idOrder].Wares[i].Count * (int)orders[idOrder].Wares[i].Price;
                    }
                }
                catch (Exception) { }

                return orderWareInfoExt;
            });

            //отдельная задачка, чтобы еще один проект не создавать консольный
            app.MapPost("/JanuaryDays", (string firstDay) => 
            {
                /*Сгенерировать массив чисел 1–31 включительно (числа месяца).
                Вывести для каждого из чисел строку «[число] января, [день недели]». 
                День недели 1 января должен задаваться входящим параметром firstDay, программа должна
                корректно работать для любого дня недели, с которого начинается месяц. 
                
                Входящий параметр - строка - например "четверг"

                Подсказка 1: для дней недели можно создать массив с названиями дней, чтобы обращаться к нему по индексу.
                Подсказка 2: индекс дня недели можно вычислить с помощью операции остатка от деления.

                Проверка результата:
                Для любого указанного дня недели 1 января все дни выводятся
                корректно. Например, для вторника:
                1 января, вторник
                2 января, среда
                3 января, четверг
                4 января, пятница
                5 января, суббота
                6 января, воскресенье
                7 января, понедельник
                и т.д.*/

                string[] days = new string[] { "понедельник", "вторник", "среда", "четверг", "пятница", "суббота", "воскресенье" };

                int dayId = Array.IndexOf(days, firstDay);

                if (dayId != -1)
                {
                    for (int i = 0; i < 31; i++)
                    {
                        Console.WriteLine($"{i + 1} января, {days[dayId]}");

                        dayId++;

                        if (dayId > 6) dayId = 0;
                    }
                }
                else { Console.WriteLine($"Неправильный день"); }
            });

            app.Run();
        }
    }
}