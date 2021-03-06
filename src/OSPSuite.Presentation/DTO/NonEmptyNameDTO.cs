﻿using System.Linq;
using OSPSuite.Assets;
using OSPSuite.Utility.Validation;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Extensions;

namespace OSPSuite.Presentation.DTO
{
   public class NonEmptyNameDTO : ValidatableDTO
   {
      private string _name;
      public string OriginalName { get; set; }

      public virtual string Name
      {
         get { return _name; }
         set
         {
            _name = value.TrimmedValue();
            OnPropertyChanged(() => Name);
         }
      }

      public NonEmptyNameDTO()
      {
         Rules.Add(AllRules.NameNotEmpty);
         Rules.Add(AllRules.NameDoesNotContainIllegalCharacters);
      }

      private static bool nameDoesNotContainerIllegalCharacters(string name)
      {
         if (string.IsNullOrEmpty(name))
            return true;

         return !Constants.ILLEGAL_CHARACTERS.Any(name.Contains);
      }

      protected static class AllRules
      {
         public static IBusinessRule NameNotEmpty
         {
            get { return GenericRules.NonEmptyRule<NonEmptyNameDTO>(x => x.Name, Error.NameIsRequired); }
         }

         public static IBusinessRule NameDoesNotContainIllegalCharacters
         {
            get
            {
               return CreateRule.For<NonEmptyNameDTO>()
                  .Property(item => item.Name)
                  .WithRule((dto, name) => nameDoesNotContainerIllegalCharacters(name))
                  .WithError(Error.NameCannotContainIllegalCharacters(Constants.ILLEGAL_CHARACTERS));
            }
         }

         public static IBusinessRule NameShouldNotBeTheSame
         {
            get
            {
               return CreateRule.For<NonEmptyNameDTO>()
                  .Property(x => x.Name)
                  .WithRule((dto, name) => (string.IsNullOrEmpty(dto.OriginalName) || !string.Equals(dto.OriginalName, name.Trim())))
                  .WithError(Error.RenameSameNameError);
            }
         }
      }
   }
}