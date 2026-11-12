using System;
using System.Collections.Generic;

namespace UFQ.UseCases.Events
{
    /// <summary>Bus de eventos del juego: las capas se comunican sin conocerse.</summary>
    public interface IEventBus
    {
        IDisposable Subscribe<T>(Action<T> handler);
        void Publish<T>(T message);
    }

    /// <summary>Implementación síncrona en memoria, sin dependencias de Unity.</summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (!_handlers.TryGetValue(typeof(T), out var list))
            {
                list = new List<Delegate>();
                _handlers[typeof(T)] = list;
            }

            list.Add(handler);
            return new Subscription(() => list.Remove(handler));
        }

        public void Publish<T>(T message)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;

            // Copia: un handler puede desuscribirse mientras se publica.
            foreach (var handler in list.ToArray())
            {
                ((Action<T>)handler)(message);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private Action _dispose;
            public Subscription(Action dispose) => _dispose = dispose;

            public void Dispose()
            {
                _dispose?.Invoke();
                _dispose = null;
            }
        }
    }
}
