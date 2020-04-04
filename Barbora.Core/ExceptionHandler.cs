using System;

namespace Barbora.Core
{
    public interface IExceptionHandler
    {
        void Handle(Exception exc);
    }

    public class ExceptionHandler : IExceptionHandler
    {
        private Action<Exception> exceptionHandlerAction;

        public ExceptionHandler(Action<Exception> exceptionHandlerAction)
        {
            this.exceptionHandlerAction = exceptionHandlerAction;
        }

        public void Handle(Exception exc)
        {
            exceptionHandlerAction(exc);
        }
    }
}