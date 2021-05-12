using System;

namespace PoEStashSorterModels.Exceptions
{
    public class CharacterInfoException : Exception
    {
        public CharacterInfoException(string message) : base(message)
        {
        }

        public CharacterInfoException() : base("Wrong login/password or SID. If login protection try get SID via steam")
        {
        }
    }
}