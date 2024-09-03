public void Main()
{
    // Get the current XML node passed to the script
    GetVarables();

    // Process the node to determine if the file should be copied
    ParseNode();

    // Set the output variables based on the results of ParseNode
    SetReturnVarables();

    // Set the task result to success if everything ran correctly
    Dts.TaskResult = (int)ScriptResults.Success;
}

public void GetVarables()
{
    // Retrieve the current node being passed into the script
    m_oNode = (System.Xml.XmlNode)Dts.Variables["Node"].Value;
}

public void SetReturnVarables()
{
    if (m_bCopyFile)
    {
        Dts.Variables["bCopyFile"].Value = m_bCopyFile;

        // Set the variables from the selected XML node.
        Dts.Variables["User::sSourceDirectory"].Value = m_oNode.SelectSingleNode("SourceDirectory").InnerText;
        Dts.Variables["User::sSourceFileName"].Value = m_oNode.SelectSingleNode("SourceFileName").InnerText;
        Dts.Variables["User::sDestination"].Value = m_oNode.SelectSingleNode("DestinationDirectory").InnerText;
        Dts.Variables["User::sDestinationFileName"].Value = m_oNode.SelectSingleNode("DestinationFileName").InnerText;
    }
    else
    {
        // Optionally set the task result to failure if no action is taken
        Dts.TaskResult = (int)ScriptResults.Failure;
    }
}

public void ParseNode()
{
    DateTime _DT = DateTime.Today;
    DayOfWeek _TodayDayOfWeek = _DT.DayOfWeek;

    bool _bAction = true;
    m_bCopyFile = true;

    if (m_oNode.Attributes["schedule"] != null)
    {
        string _days = m_oNode.Attributes["schedule"].Value.ToString();

        if (_days != "All")
        {
            switch (_TodayDayOfWeek)
            {
                case DayOfWeek.Sunday:
                    _bAction = (_days.IndexOf("Su") > -1);
                    break;
                case DayOfWeek.Monday:
                    _bAction = (_days.IndexOf("Mo") > -1);
                    break;
                case DayOfWeek.Tuesday:
                    _bAction = (_days.IndexOf("Tu") > -1);
                    break;
                case DayOfWeek.Wednesday:
                    _bAction = (_days.IndexOf("We") > -1);
                    break;
                case DayOfWeek.Thursday:
                    _bAction = (_days.IndexOf("Th") > -1);
                    break;
                case DayOfWeek.Friday:
                    _bAction = (_days.IndexOf("Fr") > -1);
                    break;
                case DayOfWeek.Saturday:
                    _bAction = (_days.IndexOf("Sa") > -1);
                    break;
                default:
                    _bAction = false;
                    break;
            }
        }
    }

    if (m_oNode.Attributes["start_time"] != null && _bAction)
    {
        string _start_time = m_oNode.Attributes["start_time"].Value.ToString();
        string[] _comp = _start_time.Split(':');

        DateTime _moment = DateTime.Now;

        int _runhour = Convert.ToInt32(_comp[0]);
        int _runmin = Convert.ToInt32(_comp[1]);

        int _hour = _moment.Hour;
        int _min = _moment.Minute;

        if (_runhour == _hour && (_min >= _runmin && _min <= _runmin + 5))
        {
            _bAction = true;
        }
        else
        {
            _bAction = false;
        }
    }

    m_bCopyFile = m_bCopyFile && _bAction;
}
