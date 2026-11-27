using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DataTool.Core.Tasks
{
    /// <summary>
    /// Tìm mọi class implement ITask (không abstract, có constructor không tham số)
    /// trong các assembly được đăng ký hoặc trong thư mục plugin.
    /// </summary>
    public class TaskRegistry
    {
        private readonly List<ITask> _tasks = new List<ITask>();
        private readonly List<string> _errors = new List<string>();

        public IReadOnlyList<ITask> Tasks => _tasks;

        /// <summary>Lỗi khi nạp assembly/khởi tạo task (không làm dừng chương trình).</summary>
        public IReadOnlyList<string> Errors => _errors;

        public TaskRegistry AddAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
                _errors.Add($"{assembly.GetName().Name}: {ex.LoaderExceptions.FirstOrDefault()?.Message}");
            }

            foreach (var type in types.Where(IsTaskType))
            {
                if (_tasks.Any(t => t.GetType() == type)) continue;
                try
                {
                    _tasks.Add((ITask)Activator.CreateInstance(type));
                }
                catch (Exception ex)
                {
                    _errors.Add($"{type.FullName}: {ex.GetBaseException().Message}");
                }
            }
            return this;
        }

        public TaskRegistry AddAssemblyOf<T>() => AddAssembly(typeof(T).Assembly);

        /// <summary>Nạp mọi file *.dll trong thư mục (ví dụ thư mục Tasks cạnh file exe).</summary>
        public TaskRegistry AddFolder(string folder, string searchPattern = "*.dll")
        {
            if (!Directory.Exists(folder)) return this;

            foreach (var file in Directory.GetFiles(folder, searchPattern))
            {
                try
                {
                    AddAssembly(Assembly.LoadFrom(file));
                }
                catch (Exception ex)
                {
                    _errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                }
            }
            return this;
        }

        /// <summary>Tìm theo MenuPath (không phân biệt hoa thường) hoặc theo tên class.</summary>
        public ITask Find(string pathOrTypeName)
        {
            if (string.IsNullOrWhiteSpace(pathOrTypeName)) return null;
            string key = pathOrTypeName.Trim();
            return _tasks.FirstOrDefault(t => string.Equals(t.MenuPath, key, StringComparison.OrdinalIgnoreCase))
                ?? _tasks.FirstOrDefault(t => string.Equals(t.GetType().Name, key, StringComparison.OrdinalIgnoreCase))
                ?? _tasks.FirstOrDefault(t => string.Equals(t.GetType().FullName, key, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Dựng cây menu từ MenuPath của các tác vụ.</summary>
        public IReadOnlyList<TaskMenuNode> BuildMenu()
        {
            var roots = new List<TaskMenuNode>();
            foreach (var task in _tasks)
            {
                var parts = TaskPath.Split(task.MenuPath);
                if (parts.Length == 0) continue;

                List<TaskMenuNode> level = roots;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string name = parts[i].Trim();
                    var group = level.FirstOrDefault(n => n.Task == null && n.Title == name);
                    if (group == null)
                    {
                        group = new TaskMenuNode(name, null);
                        level.Add(group);
                    }
                    level = group.ChildList;
                }
                level.Add(new TaskMenuNode(parts[parts.Length - 1].Trim(), task));
            }

            Sort(roots);
            return roots;
        }

        private static void Sort(List<TaskMenuNode> nodes)
        {
            nodes.Sort((a, b) =>
            {
                int byOrder = (a.Task?.Order ?? 0).CompareTo(b.Task?.Order ?? 0);
                return byOrder != 0 ? byOrder : string.Compare(a.Title, b.Title, StringComparison.CurrentCulture);
            });
            foreach (var n in nodes) Sort(n.ChildList);
        }

        private static bool IsTaskType(Type t) =>
            typeof(ITask).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition
            && t.GetConstructor(Type.EmptyTypes) != null;
    }

    /// <summary>Một nút menu: nhóm (Task == null) hoặc tác vụ.</summary>
    public sealed class TaskMenuNode
    {
        internal TaskMenuNode(string title, ITask task)
        {
            Title = title;
            Task = task;
        }

        public string Title { get; }
        public ITask Task { get; }
        public IReadOnlyList<TaskMenuNode> Children => ChildList;
        internal List<TaskMenuNode> ChildList { get; } = new List<TaskMenuNode>();
    }
}
