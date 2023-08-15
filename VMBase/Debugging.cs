using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMBase
{
    /// <summary>
    /// Holds debugging info for VMBase
    /// </summary>
    public static class Debugging
    {
        /// <summary>
        /// If logging is enabled, view models will be logged and you can use the Debugging class to troubleshoot
        /// </summary>
        public static bool EnableLogging { get; set; }

        /// <summary>
        /// Houses the registry of view-models
        /// </summary>
        private static HashSet<ViewModelDebugData> _vms = new HashSet<ViewModelDebugData>();

        /// <summary>
        /// Gets the amount of registered view-models
        /// </summary>
        public static int CurrentViewModelCount
        {
            get
            {
                lock (_vms)
                    return _vms.Count;
            }
        }

        /// <summary>
        /// Gets all currently registered view-models
        /// </summary>
        /// <typeparam name="ViewModelType"></typeparam>
        /// <returns></returns>
        public static IEnumerable<ViewModelType> GetViewModels<ViewModelType>()
        {
            return GetAllViewModels()
                .OfType<ViewModelType>();
        }

        /// <summary>
        /// Gets all view-models currently registered
        /// </summary>
        public static IEnumerable<object> GetAllViewModels()
        {
            lock (_vms)
                return _vms.ToArray();
        }

        /// <summary>
        /// Gets all currently registered view-models that have a particular owner item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<ViewModelBase<T>> GetViewModelsWithItem<T>(T item)
        {
            lock (_vms)
                return _vms
                    .OfType<ViewModelBase<T>>()
                    .Where(i => i.Item?.Equals(item) == true || (item == null && i.Item == null))
                    .ToArray();
        }

        /// <summary>
        /// Registers debug info about a newly-created view-model
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="stack"></param>
        internal static void AttemptRegisterDebugData(object vm)
        {
            if (!EnableLogging)
                return;

            const int REMOVED_STACK_DEPTH = 4;
            string[] stack = Environment.StackTrace
                .Replace("\r", "")
                .Split('\n')[REMOVED_STACK_DEPTH..];

            lock (_vms)
                _vms.Add(new ViewModelDebugData(vm, DateTime.Now, stack));
        }

        /// <summary>
        /// Unregisters debug info about a view-model that has been disposed
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="stack"></param>
        internal static void AttemptUnregisterDebugData(object vm)
        {
            if (!EnableLogging)
                return;

            lock (_vms)
                _vms.RemoveWhere(i => i.ViewModel == vm);
        }

        /// <summary>
        /// Dumps all debug data to a string
        /// </summary>
        /// <param name="file"></param>
        public static string Dump()
        {
            lock (_vms)
            {
                StringBuilder output = new StringBuilder();
                output.AppendLine("==View Model Report==");

                foreach (var i in _vms.GroupBy(i => i.ViewModel.GetType().FullName).OrderByDescending(i => i.Count()))
                    output.AppendLine($"{i.Key}: {i.Count()}");

                output.AppendLine("Total: " + CurrentViewModelCount);
                output.AppendLine();

                foreach (ViewModelDebugData i in _vms)
                {
                    //Oooh boy, getting freaky!
                    dynamic vm = i.ViewModel;

                    output.Append($"[{i.RegistrationTime}] ");
                    output.Append($"{i.ViewModel} (type: {i.ViewModel.GetType().Name}, item: {vm.Item}) ");

                    foreach (string call in i.RegistrationCallStack)
                        output.Append(call);

                    output.AppendJoin(' ', i.RegistrationCallStack);
                    output.AppendLine();
                }

                return output.ToString();
            }
        }

        /// <summary>
        /// Holds debugging information about a view-model
        /// </summary>
        public record ViewModelDebugData(object ViewModel, DateTime RegistrationTime, string[] RegistrationCallStack);
    }
}
