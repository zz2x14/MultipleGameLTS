using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserUI : MonoBehaviour
{
    public static UserUI Instance { get; private set; }

    #region UI
    [Header("登陆注册")]
    [SerializeField] private GameObject loginInterfaceGo;
    [SerializeField] private GameObject loginGO;
    [SerializeField] private GameObject registerGo;

    [SerializeField] private TMP_InputField aNumInputField_Login;
    [SerializeField] private TMP_InputField passwordInputField_Login;
    [SerializeField] private TMP_InputField aNumInputField_Register;
    [SerializeField] private TMP_InputField passwordInputField_Register;

    [SerializeField] private Button loginButton;
    [SerializeField] private Button goRegisterButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backLoginButotn;

    [SerializeField] private TextMeshProUGUI systemTipText;

    [SerializeField] private Toggle pwShower;

    [Header("账号角色")] 
    [SerializeField] private GameObject roleInterfaceGO;
    [SerializeField] private GameObject createRoleGo;

    [SerializeField] private Button createRoleButton;
    [SerializeField] private Button confirmCreateButton;
    [SerializeField] private Button cancalCreateButton;
    

    [SerializeField] private TMP_InputField roleNameInputField;

    [SerializeField] private TextMeshProUGUI systemTipText_Role;

    [SerializeField] private List<Image> roleContainerImageList = new List<Image>();
    [SerializeField] private List<Image> roleImageList = new List<Image>();
    [SerializeField] private List<TextMeshProUGUI> roleNameList = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> roleIDList = new List<TextMeshProUGUI>();
   
    [Header("系统")]
    [SerializeField] private Button startGameButton;
    #endregion
   
    #region SystemTipContent
    private const string TIP_WRONGPW = "密码错误！";
    private const string TIP_ISANUMNULL = "账号不能为空！";
    private const string TIP_ISPWNULL = "密码不能为空！";
    private const string TIP_MINANUM = "账号最短为6位数哦";
    private const string TIP_MINPW = "密码最短为6位数哦";
    private const string TIP_NOTEXISITENT = "账号不存在！";
    
    private const string TIP_REPEATEDNUM = "该账号已经存在！";
    private const string TIP_REGISTER = "注册成功！";

    private const string TIP_OVERTIME = "响应超时，请重试！";

    private const string TIP_ISNULLNAME = "名字不能为空";
    private const string TIP_REPEATEDNAME = "角色名已存在！";
    private const string TIP_ROLEMAX = "角色数量已达最大数量";

    private const string TIP_NOROLE = "没有角色！";
    #endregion
    
    private string accountNum;
    private string password;
    private bool showPW = false;

    private const int MIN_NUMBERCOUNT = 6;
    private const int MAX_ROLECOUNT = 4;

    private const float TIME_RESPONSE = 8;
    private bool isWaiting = true;
    private int responseID;
    
    private string newRoleName;
    //TODO:需要一个专门管理玩家账号和角色信息的类 每次进入时通过服务端检测账号角色信息
    private int curRoleCount = -1;
    public int NewRoleID { get; set; }

    public Color curRoleColor { get; set; }
    public string curRoleName { get; set; }
    
    //TODO:每次进入游戏的NetID也该放在角色管理类当中
    public int NetID { get; set; } = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnEnable()
    {
        pwShower.onValueChanged.AddListener(OnPasswordShowerToggleValueChange);
        passwordInputField_Login.onSelect.AddListener(OnLoginPasswordInputFieldSelect);
    }

    private void OnDisable()
    {
        pwShower.onValueChanged.RemoveAllListeners();
        passwordInputField_Login.onSelect.RemoveAllListeners();
    }

    #region USER_FUNCTIONS
    public void OnLoginButtonClick()
    {
        if (string.IsNullOrWhiteSpace(aNumInputField_Login.text))
        {
            SystemTip(systemTipText,TIP_ISANUMNULL);
            ClearLoginContent();
        }
        else
        {
            if (aNumInputField_Login.text.Length < MIN_NUMBERCOUNT)
            {
                SystemTip(systemTipText,TIP_MINANUM);
                ClearLoginContent();
            }
            else if (string.IsNullOrWhiteSpace(passwordInputField_Login.text))
            {
                SystemTip(systemTipText,TIP_ISPWNULL);
                ClearLoginContent();
            }
            else
            {
                if (passwordInputField_Login.text.Length < MIN_NUMBERCOUNT)
                {
                    SystemTip(systemTipText,TIP_MINPW);
                    ClearLoginContent();
                }
                else
                {
                    UserNetMsg userNetMsg = new UserNetMsg { UserID = 0,AccountNum = accountNum,Password = password};
                    NetMgr.Instance.BeginSend(userNetMsg);
                    StartCoroutine(nameof(LoginWaitForServerResponseCor));
                }
            }
        }
    }
    public void OnGoRegisterButtonClick()
    {   
        ClearLoginContent();
        loginGO.SetActive(false);
        registerGo.SetActive(true);
    }
    public void OnRegisterButtonClick()
    {
        if (string.IsNullOrWhiteSpace(aNumInputField_Register.text))
        {
            SystemTip(systemTipText,TIP_ISANUMNULL);
            ClearRegisterContent();
        }
        else
        {
            if (aNumInputField_Register.text.Length < MIN_NUMBERCOUNT)
            {
                SystemTip(systemTipText,TIP_MINANUM);
                ClearRegisterContent();
            }
            else if (string.IsNullOrWhiteSpace(passwordInputField_Register.text))
            {
                SystemTip(systemTipText,TIP_ISPWNULL);
                ClearRegisterContent();
            }
            else
            {
                if (passwordInputField_Register.text.Length < MIN_NUMBERCOUNT)
                {
                    SystemTip(systemTipText,TIP_MINPW);
                    ClearRegisterContent();
                }
                else
                {
                    UserNetMsg userNetMsg = new UserNetMsg { UserID = 0,AccountNum = accountNum,Password = password,DoRegister = true};
                    NetMgr.Instance.BeginSend(userNetMsg);
                    StartCoroutine(nameof(RegisterWaitForServerResponseCor));
                }
            }
        }
    }
    public void OnBackButtonClick()
    {
        ClearRegisterContent();
        registerGo.SetActive(false);
        loginGO.SetActive(true);
    }

    public void OnPasswordShowerToggleValueChange(bool show)
    {
        showPW = show;
        
        passwordInputField_Login.contentType =
            showPW ? TMP_InputField.ContentType.Alphanumeric : TMP_InputField.ContentType.Password;
        
        passwordInputField_Login.ForceLabelUpdate();
    }

    public void OnLoginANumInputFieldEndEdit()
    {
        accountNum = aNumInputField_Login.text;
        print(accountNum);
    }
    public void OnLoginPasswordInputFieldEndEdit()
    {
        password = passwordInputField_Login.text;
        print(password);
    }
    public void OnRegisterANumInputFieldEndEdit()
    {
        accountNum = aNumInputField_Register.text;
        print(accountNum);
    }
    public void OnRegisterPasswordInputFieldEndEdit()
    {
        password = passwordInputField_Register.text;
        print(password);
    }
    public void OnLoginPasswordInputFieldSelect(string content)
    {
        if (showPW)
        {
            passwordInputField_Login.contentType = TMP_InputField.ContentType.Password;
        }
    }
    
    IEnumerator LoginWaitForServerResponseCor()
    {
        float t = 0;
        while (isWaiting)
        {
            if (t >= TIME_RESPONSE)
            {
                SystemTip(systemTipText,TIP_OVERTIME);
                break;
            }
              
            t += Time.deltaTime;

            yield return null;
        }

        switch (responseID)
        {
            case MsgHandler.ID_RESPONSE_LOGIN:
                ClearLoginContent();
                loginInterfaceGo.SetActive(false);
                roleInterfaceGO.SetActive(true);
                break;
            case MsgHandler.ID_RESPONSE_NOTEXISTENT:
                SystemTip(systemTipText,TIP_NOTEXISITENT);
                ClearLoginContent();
                break;
            case MsgHandler.ID_RESPONSE_WRONG:
                SystemTip(systemTipText,TIP_WRONGPW);
                ClearLoginContent();
                break;
        }

        Reset();
    }
    IEnumerator RegisterWaitForServerResponseCor()
    {
        float t = 0;
        while (isWaiting)
        {
            if (t >= TIME_RESPONSE)
            {
                SystemTip(systemTipText,TIP_OVERTIME);
                break;
            }
              
            t += Time.deltaTime;

            yield return null;
        }

        switch (responseID)
        {
           case MsgHandler.ID_RESPONSE_REGISTER:
               SystemTip(systemTipText,TIP_REGISTER);
               aNumInputField_Login.text = accountNum;
               ClearRegisterContent();
               registerGo.SetActive(false);
               loginGO.SetActive(true);
               break;
           case MsgHandler.ID_RESPONSE_REPEATEDNUM:
               SystemTip(systemTipText,TIP_REPEATEDNUM);
               ClearRegisterContent();
               break;
        }

        Reset();
    }
    
    private void ClearLoginContent()
    {
        aNumInputField_Login.text = null;
        passwordInputField_Login.text = null;
    }
    private void ClearRegisterContent()
    {
        aNumInputField_Register.text = null;
        passwordInputField_Register.text = null;
    }
    #endregion

    #region ROLE_FUNCTIONS
    public void OnCreateRoleButtonClick()
    {
        if (curRoleCount >= MAX_ROLECOUNT)
        {
            SystemTip(systemTipText_Role,TIP_ROLEMAX);
            return;
        }
        
        curRoleCount = Mathf.Min(curRoleCount + 1, MAX_ROLECOUNT);
        
        startGameButton.gameObject.SetActive(false);
        createRoleButton.gameObject.SetActive(false);
        createRoleGo.SetActive(true);
    }
    public void OnConfirmCreateRoleButtonClick()
    {
        if (string.IsNullOrEmpty(roleNameInputField.text))
        {
            SystemTip(systemTipText_Role, TIP_ISNULLNAME);
        }
        else
        {
            CreateRoleNetMsg createRoleNetMsg = new CreateRoleNetMsg
            {
                UserID = NetMgr.Instance.UserID,
                RoleName = newRoleName
            };
            NetMgr.Instance.BeginSend(createRoleNetMsg);
            StartCoroutine(nameof(RoleWaitServerResponseCor));
        }
    }
    public void OnCancelCreateButtonClick()
    {
        curRoleCount = Mathf.Max(curRoleCount - 1, -1);
        
        roleNameInputField.text = null;
        createRoleGo.SetActive(false);
        createRoleButton.gameObject.SetActive(true);
        startGameButton.gameObject.SetActive(true);
    }

    public void OnRoleNameInputFieldEndEdit()
    {
        newRoleName = roleNameInputField.text;
    }
    
    IEnumerator RoleWaitServerResponseCor()
    {
        float t = 0;
        while (isWaiting)
        {
            if (t >= TIME_RESPONSE)
            {
                SystemTip(systemTipText,TIP_OVERTIME);
                break;
            }
              
            t += Time.deltaTime;

            yield return null;
        }

        switch (responseID)
        {
            case MsgHandler.ID_RESPONSE_REPETEDNAME:
                roleNameInputField.text = null;
                SystemTip(systemTipText_Role,TIP_REPEATEDNAME);
                break;
            case MsgHandler.ID_RESPONSE_ROLE:
                roleNameInputField.text = null;
                createRoleGo.gameObject.SetActive(false);
                startGameButton.gameObject.SetActive(true);
                createRoleButton.gameObject.SetActive(true);
                break;
        }
        
        Reset();
    }
    #endregion

    public void OnStartGameButtonClick()
    {
        if (curRoleCount == -1)
        {
            SystemTip(systemTipText_Role,TIP_NOROLE);
            return;
        }
        
        startGameButton.gameObject.SetActive(false);
        createRoleButton.gameObject.SetActive(false);

        BornNetMsg bornNetMsg = new BornNetMsg
        {
            IsNetPlayer = true,
            PrefabPath = GameManager.PATH_PREFAB_PLAYER
        };
        NetMgr.Instance.BeginSend(bornNetMsg);
        
        StartCoroutine(nameof(StartGameWaitServerResponse));
    }

    IEnumerator StartGameWaitServerResponse()
    {
        float t = 0;
        while (NetID == 0)
        {
            if (t >= TIME_RESPONSE)
            {
                SystemTip(systemTipText,TIP_OVERTIME);
                startGameButton.gameObject.SetActive(true);
                createRoleButton.gameObject.SetActive(true);
                break;
            }
              
            t += Time.deltaTime;

            yield return null;
        }
        
        SceneMgr.Instance.LoadLobbyScene(NetID);
    }
   
    public void MatchResponseID(int id)
    {
        responseID = id;
        isWaiting = false;
    }

    //TODO:还会有角色类 
    public void GetRoleInfo()
    {
        roleIDList[curRoleCount].text = "角色ID：" + NewRoleID;
        roleIDList[curRoleCount].color = Color.black;
        roleNameList[curRoleCount].text = newRoleName;
        roleImageList[curRoleCount].gameObject.SetActive(true);
        
        //若为创建的第一个角色 默认使第一个角色被选中
        if (curRoleCount == 0)
        {
            roleContainerImageList[0].color = Color.black;
            curRoleColor = roleImageList[0].color;
            curRoleName = roleNameList[0].text; 
        }
    }
    
    private void Reset()
    {
        responseID = 0;
        isWaiting = true;
    }

    private void SystemTip(TextMeshProUGUI text, string content)
    {
        text.transform.parent.gameObject.SetActive(true);
        text.text = content;
    }

    public void SelectOneRole(int index)
    {
        curRoleColor = roleImageList[index].color;
        curRoleName = roleNameList[index].text;
        
        for (int i = 0; i < roleContainerImageList.Count; i++)
        {
            roleContainerImageList[i].color = index == i ? Color.black : Color.white;
        }
    }
}
