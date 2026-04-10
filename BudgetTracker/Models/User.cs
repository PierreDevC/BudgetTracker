using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.Models;
public class User
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;      // "but"
    public string Reason { get; set; } = string.Empty;     // "raison"
}
