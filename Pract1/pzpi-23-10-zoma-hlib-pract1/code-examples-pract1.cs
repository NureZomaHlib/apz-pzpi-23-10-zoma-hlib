//Напиши код на C#, що реалізує патерн Iterator для обходу колекції об'єктів Employee (у них є дані ПІБ та посада).
//Створи абстрактні класи Iterator (наслідується від IEnumerator) та IteratorAggregate (від IEnumerable).
//Реалізуй клас Department, який зберігає список співробітників і з функцією зміни напрямку обходу,
//а також клас DepartmentIterator, у якому є логіка прямого та зворотного перебору елементів.
//У методі Main протестуй роботу за допомогою циклу foreach.

using System.Collections;

namespace APZ_PZ1;

public class Employee
{
    public string FullName { get; set; }
    public string Position { get; set; }

    public Employee(string fullName, string position)
    {
        FullName = fullName;
        Position = position;
    }

    public override string ToString()
    {
        return $"{FullName} ({Position})";
    }
}

public abstract class Iterator : IEnumerator
{
    object IEnumerator.Current => Current();

    public abstract int Key();
    public abstract object Current();
    public abstract bool MoveNext();
    public abstract void Reset();
}

public abstract class IteratorAggregate : IEnumerable
{
    public abstract IEnumerator GetEnumerator();
}

public class Department : IteratorAggregate
{
    private List<Employee> _collection = new List<Employee>();
    private bool _direction = false;

    public void ReverseDirection()
    {
        _direction = !_direction;
    }

    public List<Employee> GetItems()
    {
        return _collection;
    }

    public void AddEmployee(Employee employee)
    {
        _collection.Add(employee);
    }

    public override IEnumerator GetEnumerator()
    {
        return new DepartmentIterator(this, _direction);
    }
}

public class DepartmentIterator : Iterator
{
    private Department _collection;
    private int _position = -1;
    private bool _reverse = false;

    public DepartmentIterator(Department collection, bool reverse = false)
    {
        _collection = collection;
        _reverse = reverse;
        if (reverse)
            _position = collection.GetItems().Count;
    }

    public override object Current() => _collection.GetItems()[_position];

    public override int Key() => _position;

    public override bool MoveNext()
    {
        int updatedPosition = _position + (_reverse ? -1 : 1);
        if (updatedPosition >= 0 && updatedPosition < _collection.GetItems().Count)
        {
            _position = updatedPosition;
            return true;
        }

        return false;
    }

    public override void Reset() =>
        _position = _reverse ? _collection.GetItems().Count - 1 : 0;
}

class Program
{
    static void Main()
    {
        Department itDepartment = new Department();
        itDepartment.AddEmployee(new Employee("Олександр Іванов", "Software Engineer"));
        itDepartment.AddEmployee(new Employee("Марія Петренко", "Project Manager"));

        Console.WriteLine("Прямий обхід:");
        foreach (Employee element in itDepartment)
            Console.WriteLine(element);

        Console.WriteLine("\nЗворотний обхід:");
        itDepartment.ReverseDirection();
        foreach (Employee element in itDepartment)
            Console.WriteLine(element);
    }
}