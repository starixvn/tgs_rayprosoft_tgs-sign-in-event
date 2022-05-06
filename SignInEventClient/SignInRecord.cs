using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgs.SignInEvents.Clients;

public record struct SignInRecord(Guid Id, int Combo);
