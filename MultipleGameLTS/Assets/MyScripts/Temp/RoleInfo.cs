using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleInfo : MonoBehaviour
{
    private Button roleButton;

    private void Awake()
    {
        roleButton = GetComponentInChildren<Button>(true);
    }

    private void OnEnable()
    {
        roleButton.onClick.AddListener(OnRoleButtonClick);
    }

    private void OnDisable()
    {
        roleButton.onClick.RemoveAllListeners();
    }

    public void OnRoleButtonClick()
    {
        UserUI.Instance.SelectOneRole(transform.GetSiblingIndex());
    }
}
