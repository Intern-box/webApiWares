namespace webApiWares
{
    // Строчка с товаром в заказе
    public class OrderWareInfo
    {
        static List<int> Counter; // База Id товаров
        public int IdWare; // Id товара в каталоге товаров
        public string Name = string.Empty; //Наименование товара
        public int Count { get; set; } //Количество в заказе
        public decimal Price { get; set; } //Цена за 1 единицу товара
        public int Total; // Count * Price

        public OrderWareInfo()
        {
            if (IdWare == 0) RunCounter(); // Id товара НЕ может равняться нулю!

            if (Counter.Contains(IdWare)) RunCounter(); // Если Id нового товара совпадает с Id другого товара, генерируется другой Id

            Total = Count * (int)Price;
        }

        void RunCounter() // Запустили Counter
        {
            int random = new Random().Next(0, 1000000); // Инициируем переменную

            while (Counter.Contains(random)) random = new Random().Next(0, 1000000); // Определяем есть ли совпадения по Id

            IdWare = random; // Инициируем Id товара

            Counter.Add(random); // Записываем Id товара в базу
        }
    }

    // Описание заказа
    public class Order
    {
        public int Id { get; set; } // Номер заказа
        public DateTime Date { get; set; } //Дата создания
        public List<OrderWareInfo>? Wares { get; set; } //Список товаров в заказе
        public int countWares; // Кол-во товаров в заказе
        public Order() { if (Wares is not null) countWares = Wares.Count; }
    }
    
    // Заказы
    public class DataContext
    {
        public List<string> Wares { get; set; } = new List<string>(); // Каталог товаров
        public List<Order> Orders { get; set; } = new List<Order>(); // Список заказов

        public DataContext()
        {
            // Добавление товаров в Каталог товаров
            Wares.Add("Зеленка"); Wares.Add("Йод");

            // Формирование заказов
            Order order1 = new()
            {
                Id = 1,
                Date = DateTime.Now,
                Wares = new()
                        {
                            new() {Name = "Зеленка", Count = 5, Price = 15.20m },
                            new() {Name = "Йод", Count = 10, Price = 10.50m }
                        }
            };

            Order order2 = new()
            {
                Id = 2,
                Date = new DateTime(2023, 5, 1),
                Wares = new()
                        {
                            new() {Name = "Зеленка", Count = 50, Price = 10.50m },
                            new() {Name = "Йод", Count = 100, Price = 9.50m }
                        }
            };

            // Добавление заказов в Список заказов
            Orders.Add(order1); Orders.Add(order2);
        }
    }
}