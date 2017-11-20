using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour 
{
#if UNITY_EDITOR
    [MenuItem("Helpers/Clear Data")]
    public static void ClearData()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Helpers/Show Current Data")]
    public static void ShowData()
    {
        DataManager.PrintCurrentData();
    }
#endif
    [Header("Locations")]
    public InputField locationsInput;
    public Text locationsMessage;

    [Header("Temperature")]
    public Dropdown temperatureDropDown;
    public InputField temperatureInput;
    public Text temperatureMessage;

    [Header("Report")]
    public Transform reportContent;
    public GameObject reportTextFieldPrefab;
    public InputField dateFrom;
    public InputField dateTo;
    public Button showReportButton;
    public Button sendEmailButton;
    public Text reportMessage;

    private void Start()
    {
        dateFrom.onValueChanged.AddListener(OnDateTextValueChanged);
        dateTo.onValueChanged.AddListener(OnDateTextValueChanged);

        locationsInput.onValueChanged.AddListener(v => locationsMessage.text = string.Empty);
        
        temperatureDropDown.onValueChanged.AddListener(v => temperatureMessage.text = string.Empty);
        temperatureInput.onValueChanged.AddListener(v => temperatureMessage.text = string.Empty);

        dateFrom.onValueChanged.AddListener(v => reportMessage.text = string.Empty);
        dateTo.onValueChanged.AddListener(v => reportMessage.text = string.Empty);

        FillTemperatureDropdown();
        FillInitialDates();
    }
    
    private void FillTemperatureDropdown()
    {
        if (temperatureDropDown.options.Count > 0)
        {
            temperatureDropDown.options.Clear();
        }

        List<Dropdown.OptionData> itemList = new List<Dropdown.OptionData>();

        foreach (string location in DataManager.Locations)
        {
            Dropdown.OptionData data = new Dropdown.OptionData();
            data.text = location;
            itemList.Add(data);
        }

        temperatureDropDown.AddOptions(itemList);
    }

    private void FillInitialDates()
    {
        System.DateTime monthAgo = System.DateTime.Now.AddMonths(-1);
        dateFrom.text = monthAgo.Day.ToString() + "/" + monthAgo.Month.ToString() + "/" + monthAgo.Year.ToString();
        dateTo.text = System.DateTime.Now.Day.ToString() + "/" + System.DateTime.Now.Month.ToString() + "/" + System.DateTime.Now.Year.ToString();
    }

    public void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    #region Lokacije

    public void AddLocation(Text location)
    {
        if (!location.text.Equals(string.Empty))
        {
            locationsMessage.text = DataManager.AddLocation(location.text);
            FillTemperatureDropdown();
        }
        else
        {
            locationsMessage.text = "Unesi naziv lokacije!";
        }
    }

    #endregion

    #region Temperature

    public void AddTemperature(Text temperature)
    {
        if (!temperature.text.Equals(string.Empty))
        {
            string location = temperatureDropDown.captionText.text;
            temperatureMessage.text = DataManager.AddTemperature(location, float.Parse(temperature.text));
        }
        else
        {
            temperatureMessage.text = "Unesi temperaturu!"; 
        }
    }

    #endregion

    #region Izvjestaj

    List<string> reportStrings;
    public void PrintReport()
    {
        if (reportContent.childCount > 0)
        {
            for (int i = 0; i < reportContent.childCount; i++)
            {
                Destroy(reportContent.GetChild(i).gameObject);
            }
        }

        reportStrings = DataManager.GetReport(System.DateTime.ParseExact(dateFrom.text, "d/M/yyyy", CultureInfo.InvariantCulture), System.DateTime.ParseExact(dateTo.text, "d/M/yyyy", CultureInfo.InvariantCulture));

        foreach (string reportString in reportStrings)
        {
            GameObject reportItem = Instantiate(reportTextFieldPrefab, reportContent);
            Text itemTextComponent = reportItem.GetComponent<Text>();
            itemTextComponent.text = reportString;
        }

        sendEmailButton.interactable = true;
    }

    public void SendEmail()
    {
        MailAddress from = new MailAddress("djovanovic1980@mail.com", "Zokajlo");
        MailAddress to = new MailAddress("nexxogen@gmail.com", "Nexx");

        const string FROM_PASSWORD = "kafzmabd3";
        const string SUBJECT = "Temperature sa termometara";

        SmtpClient client = new SmtpClient()
        {
            Host = "smtp.mail.com",
            Port = 587,
            EnableSsl = false,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(from.Address, FROM_PASSWORD) as ICredentialsByHost
        };

        StringBuilder builder = new StringBuilder();

        foreach (string reportString in reportStrings)
        {
            builder.Append(reportString);
            builder.Append("\n");
        }

        using (MailMessage mail = new MailMessage(from, to)
        {
            Subject = SUBJECT,
            Body = builder.ToString()
        })
        {
            try
            {
                client.Send(mail);
                reportMessage.text = "Mail uspjesno poslat";
            }
            catch (System.Exception e)
            {
                Debug.Log("Exception message: " + e.Message + "\nInner exception: " + e.InnerException);
                reportMessage.text = "Greska prilikom slanja maila";
            }
        }
    }

    private void OnDateTextValueChanged(string newValue)
    {
        try
        {
            System.DateTime.ParseExact(newValue, "d/M/yyyy", CultureInfo.InvariantCulture);
            showReportButton.interactable = true;
        }
        catch (System.Exception)
        {
            showReportButton.interactable = false;
        }
    }

    #endregion
}
