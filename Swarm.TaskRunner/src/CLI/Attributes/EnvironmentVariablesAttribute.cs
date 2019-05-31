using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Swarm.TaskRunner.CLI.Attributes {
  [AttributeUsage(AttributeTargets.Property)]
  class EnvironmentVariablesAttribute : Attribute, IMemberConvention, McMaster.Extensions.CommandLineUtils.Validation.IOptionValidator {

    public void Apply(ConventionContext context, MemberInfo member) {
      var opt = context.Application.Option("-e|--env", "Environment Variables, input like '-e NAME=foo -e EMAIL=foo@bar.com'", CommandOptionType.MultipleValue);
      opt.Validators.Add(this);

      context.Application.OnParsingComplete(_ => {
        var inputs = (member as PropertyInfo).GetValue(context.ModelAccessor.GetModel()) as Dictionary<string, string>;

        foreach (var item in opt.Values) {
          var arr = item.Split('=');

          if (arr.Length == 2) {
            inputs.Add(arr[0].Trim(), arr[1].Trim());
          }
        }
      });
    }

    public ValidationResult GetValidationResult(CommandOption option, ValidationContext context) {
      foreach (var value in option.Values) {
        if (!(value is string input)) {
          return new ValidationResult($"Invalid environment variable for {value}");
        }

        if (input.Split('=').Length != 2) {
          return new ValidationResult($"Invalid environment variable for '{input}'");
        }
      }

      return ValidationResult.Success;
    }
  }
}