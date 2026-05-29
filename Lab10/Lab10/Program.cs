using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacultyLifeAdvanced
{
    public class FacultyTask
    {
        public string Description { get; set; }
        public string DepartmentName { get; set; }

        public FacultyTask(string desc, string dept)
        {
            Description = desc;
            DepartmentName = dept;
        }
    }

    public class FacultyDayEventArgs : EventArgs
    {
        public int Day { get; }
        public FacultyDayEventArgs(int day) { Day = day; }
    }

    public class Faculty
    {
        private string _facultyName;
        private int _simulationDays;
        private Random _rnd = new Random();

        public event EventHandler<FacultyDayEventArgs> FacultyDay;

        private PriorityQueue<FacultyTask, int> _taskQueue = new PriorityQueue<FacultyTask, int>();

        private Dictionary<string, int> _statistics = new Dictionary<string, int>();

        public Faculty(string name, int days)
        {
            _facultyName = name;
            _simulationDays = days;
        }

        public void EnqueueTask(FacultyTask task, int priority)
        {
            _taskQueue.Enqueue(task, priority);
            Console.WriteLine($"     [+] У чергу додано завдання: {task.Description} (Пріоритет: {priority})");
        }

        public async Task SimulateLifeAsync()
        {
            Console.WriteLine($"\n=== Початок моделювання: Факультет '{_facultyName}' ({_simulationDays} днів) ===");

            for (int day = 1; day <= _simulationDays; day++)
            {
                if (_rnd.NextDouble() < 0.05 || day % 30 == 0)
                {
                    Console.WriteLine($"\n---> ДЕНЬ {day}: Оголошено День факультету! <---");

                    FacultyDay?.Invoke(this, new FacultyDayEventArgs(day));

                    await ProcessTasksAsync();
                }
            }

            PrintStatistics();
        }

        private async Task ProcessTasksAsync()
        {
            Console.WriteLine("   -> Починаємо асинхронну обробку завдань з черги...");

            while (_taskQueue.Count > 0)
            {
                FacultyTask currentTask = _taskQueue.Dequeue();

                await Task.Delay(_rnd.Next(100, 500));

                Console.WriteLine($"   [Виконано] {currentTask.DepartmentName}: {currentTask.Description}");

                if (!_statistics.ContainsKey(currentTask.DepartmentName))
                    _statistics[currentTask.DepartmentName] = 0;

                _statistics[currentTask.DepartmentName]++;
            }
            Console.WriteLine("   -> Усі завдання на цей день виконані.\n");
        }

        private void PrintStatistics()
        {
            Console.WriteLine($"\n=== СТАТИСТИКА ЗА {_simulationDays} ДНІВ ===");
            if (_statistics.Count == 0)
            {
                Console.WriteLine("Жодних завдань не було виконано.");
            }
            else
            {
                foreach (var stat in _statistics)
                {
                    Console.WriteLine($"- {stat.Key}: виконано завдань - {stat.Value}");
                }
            }
            Console.WriteLine("=====================================\n");
        }
    }

    public abstract class FacultyDepartment
    {
        protected Faculty _faculty;
        protected string _departmentName;

        public FacultyDepartment(Faculty faculty, string name)
        {
            _faculty = faculty;
            _departmentName = name;
        }

        public void TurnOn() => _faculty.FacultyDay += HandleFacultyDay;
        public void TurnOff() => _faculty.FacultyDay -= HandleFacultyDay;

        public abstract void HandleFacultyDay(object sender, FacultyDayEventArgs e);
    }

    public class Deanery : FacultyDepartment
    {
        public Deanery(Faculty faculty) : base(faculty, "Деканат") { }

        public override void HandleFacultyDay(object sender, FacultyDayEventArgs e)
        {
            _faculty.EnqueueTask(new FacultyTask("Підписати наказ про вихідний", _departmentName), 1);
            _faculty.EnqueueTask(new FacultyTask("Підготувати промову Декана", _departmentName), 1);
        }
    }

    public class Accounting : FacultyDepartment
    {
        public Accounting(Faculty faculty) : base(faculty, "Бухгалтерія") { }

        public override void HandleFacultyDay(object sender, FacultyDayEventArgs e)
        {
            _faculty.EnqueueTask(new FacultyTask("Виділити кошти на премії", _departmentName), 2);
        }
    }

    public class StudentCouncil : FacultyDepartment
    {
        public StudentCouncil(Faculty faculty) : base(faculty, "Студрада") { }

        public override void HandleFacultyDay(object sender, FacultyDayEventArgs e)
        {
            _faculty.EnqueueTask(new FacultyTask("Замовити піцу", _departmentName), 3);
            _faculty.EnqueueTask(new FacultyTask("Налаштувати музичну апаратуру", _departmentName), 3);
            _faculty.EnqueueTask(new FacultyTask("Провести конкурс", _departmentName), 3);
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Faculty csFaculty = new Faculty("Кібернетики та Інженерії", 100);

            Deanery deanery = new Deanery(csFaculty);
            Accounting accounting = new Accounting(csFaculty);
            StudentCouncil studentCouncil = new StudentCouncil(csFaculty);

            deanery.TurnOn();
            accounting.TurnOn();
            studentCouncil.TurnOn();

            await csFaculty.SimulateLifeAsync();

            Console.WriteLine("Натисніть Enter для виходу...");
            Console.ReadLine();
        }
    }
}