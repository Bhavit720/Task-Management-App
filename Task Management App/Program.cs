using System;
using System.Collections.Generic;
using System.IO;

namespace TaskManagementApp
{
    class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public DateTime DueDate { get; set; }

        public TaskItem(string title, string description, int priority, DateTime dueDate)
        {
            Title = title;
            Description = description;
            Priority = priority;
            DueDate = dueDate;
        }

        public override string ToString()
        {
            return $"[{Priority}] {Title} - {Description} (Due: {DueDate.ToShortDateString()})";
        }
    }

    class PriorityQueue
    {
        private List<TaskItem> heap = new List<TaskItem>();

        private int Parent(int i) => (i - 1) / 2;
        private int LeftChild(int i) => 2 * i + 1;
        private int RightChild(int i) => 2 * i + 2;

        public void Insert(TaskItem task)
        {
            heap.Add(task);
            int index = heap.Count - 1;
            while (index > 0 && heap[Parent(index)].Priority > heap[index].Priority)
            {
                (heap[Parent(index)], heap[index]) = (heap[index], heap[Parent(index)]);
                index = Parent(index);
            }
        }

        public TaskItem ExtractMin()
        {
            if (heap.Count == 0) return null;
            TaskItem min = heap[0];
            heap[0] = heap[^1];
            heap.RemoveAt(heap.Count - 1);
            MinHeapify(0);
            return min;
        }

        private void MinHeapify(int i)
        {
            int left = LeftChild(i), right = RightChild(i), smallest = i;

            if (left < heap.Count && heap[left].Priority < heap[smallest].Priority)
                smallest = left;
            if (right < heap.Count && heap[right].Priority < heap[smallest].Priority)
                smallest = right;
            if (smallest != i)
            {
                (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
                MinHeapify(smallest);
            }
        }

        public bool IsEmpty() => heap.Count == 0;
        public List<TaskItem> GetTasks() => new List<TaskItem>(heap);
    }

    class CompletedTaskNode
    {
        public TaskItem Task { get; set; }
        public CompletedTaskNode Next { get; set; }
        public CompletedTaskNode(TaskItem task) => Task = task;
    }

    class CompletedTaskList
    {
        private CompletedTaskNode head;

        public void AddCompletedTask(TaskItem task)
        {
            var newNode = new CompletedTaskNode(task) { Next = head };
            head = newNode;
        }

        public void DisplayCompletedTasks()
        {
            Console.WriteLine("\n===== COMPLETED TASKS =====");
            CompletedTaskNode temp = head;
            while (temp != null)
            {
                Console.WriteLine(temp.Task);
                temp = temp.Next;
            }
        }
    }

    class DataHandler
    {
        private const string FilePath = "tasks.txt";

        public static void SaveTasks(PriorityQueue taskQueue)
        {
            using StreamWriter writer = new StreamWriter(FilePath);
            foreach (var task in taskQueue.GetTasks())
            {
                writer.WriteLine($"{task.Title}|{task.Description}|{task.Priority}|{task.DueDate}");
            }
        }

        public static void LoadTasks(PriorityQueue taskQueue)
        {
            if (!File.Exists(FilePath)) return;
            foreach (var line in File.ReadAllLines(FilePath))
            {
                var parts = line.Split('|');
                if (parts.Length == 4)
                {
                    taskQueue.Insert(new TaskItem(parts[0], parts[1], int.Parse(parts[2]), DateTime.Parse(parts[3])));
                }
            }
        }
    }

    class TaskManager
    {
        private PriorityQueue taskQueue = new PriorityQueue();
        private CompletedTaskList completedTasks = new CompletedTaskList();

        public void Run()
        {
            DataHandler.LoadTasks(taskQueue);
            while (true)
            {
                Console.Clear();
                Console.WriteLine("===== TASK MANAGEMENT SYSTEM =====");
                Console.WriteLine("1. Add Task");
                Console.WriteLine("2. View Pending Tasks");
                Console.WriteLine("3. Complete a Task");
                Console.WriteLine("4. View Completed Tasks");
                Console.WriteLine("5. Exit");
                Console.Write("Select an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        AddTask();
                        break;
                    case "2":
                        ViewTasks();
                        break;
                    case "3":
                        CompleteTask();
                        break;
                    case "4":
                        completedTasks.DisplayCompletedTasks();
                        Console.ReadKey();
                        break;
                    case "5":
                        DataHandler.SaveTasks(taskQueue);
                        return;
                    default:
                        Console.WriteLine("Invalid input. Try again.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void AddTask()
        {
            Console.Write("\nEnter Task Title: ");
            string title = Console.ReadLine();
            Console.Write("Enter Description: ");
            string description = Console.ReadLine();
            Console.Write("Enter Priority (1-5, lower is higher priority): ");
            int priority = int.Parse(Console.ReadLine());
            Console.Write("Enter Due Date (YYYY-MM-DD): ");
            DateTime dueDate = DateTime.Parse(Console.ReadLine());

            taskQueue.Insert(new TaskItem(title, description, priority, dueDate));
            Console.WriteLine("Task Added Successfully!");
            Console.ReadKey();
        }

        private void ViewTasks()
        {
            Console.WriteLine("\n===== PENDING TASKS =====");
            foreach (var task in taskQueue.GetTasks())
            {
                Console.WriteLine(task);
            }
            Console.ReadKey();
        }

        private void CompleteTask()
        {
            if (taskQueue.IsEmpty())
            {
                Console.WriteLine("No pending tasks to complete.");
                Console.ReadKey();
                return;
            }

            TaskItem completedTask = taskQueue.ExtractMin();
            completedTasks.AddCompletedTask(completedTask);
            Console.WriteLine($"Task '{completedTask.Title}' marked as completed.");
            Console.ReadKey();
        }
    }

    class Program
    {
        static void Main()
        {
            new TaskManager().Run();
        }
    }
}
