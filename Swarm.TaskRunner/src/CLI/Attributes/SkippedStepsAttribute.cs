using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Swarm.TaskRunner.CLI.Attributes {
  [AttributeUsage(AttributeTargets.Property)]
  class SkippedStepsAttribute : Attribute, IMemberConvention, McMaster.Extensions.CommandLineUtils.Validation.IOptionValidator {

    public void Apply(ConventionContext context, MemberInfo member) {
      var opt = context.Application.Option("-s|--skiped-steps <steps>", "The steps to skip, use comma to separate step number, like '1,2,3'", CommandOptionType.SingleValue);
      opt.Validators.Add(this);

      context.Application.OnParsingComplete(_ => {
        var value = opt.Value();
        if (value != null) {
          var skippedSteps = (member as PropertyInfo).GetValue(context.ModelAccessor.GetModel()) as Dictionary<int, bool>;
          var numbers = value.Split(',');
          foreach (var number in numbers) {
            int index;
            if (int.TryParse(number.Trim(), out index)) {
              // step is zero-based
              skippedSteps[index - 1] = true;
            }
          }
        }
      });
    }

    public ValidationResult GetValidationResult(CommandOption option, ValidationContext context) {
      var value = option.Value();
      if (value != null) {
        var numbers = value.Split(',');

        foreach (var number in numbers) {
          int index;
          if (!int.TryParse(number.Trim(), out index)) {
            return new ValidationResult($"Invalid step for '{number}'");
          }
        }
      }

      return ValidationResult.Success;
    }
  }
}