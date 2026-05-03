using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridControllerUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform rectTransform;


    public Canvas rootCanvas;
    public CanvasGroup canvasGroup;

    public GameObject representedItem;

    public Transform originalParent;
    public Vector2 originalAnchoredPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();


        rootCanvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        { 
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

    }



    public void Setup(InGameItem weaponInstance, Transform parent, float slotWidthPixels, float slotHeightPixels)
    {
        if (weaponInstance == null || weaponInstance.itemInstance == null)
        {
            return;
        }

        representedItem = weaponInstance.gameObject;

        transform.SetParent(parent, false);

        gameObject.GetComponent<Image>().sprite = weaponInstance.itemInstance.icon;
        


        int gridX = weaponInstance.itemInstance.x;
        int gridY = weaponInstance.itemInstance.y;
        float pixelWidth = weaponInstance.itemInstance.GetWidth() * slotWidthPixels;
        float pixelHeight = weaponInstance.itemInstance.GetHeight() * slotHeightPixels;

        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);


        float posX = gridX * slotWidthPixels;
        float posY = -(gridY * slotHeightPixels);

        rectTransform.anchoredPosition = new Vector2(posX, posY);
        // rectTransform.sizeDelta = new Vector2(pixelWidth, pixelHeight);

    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        if (representedItem == null)
            return;

        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;

        //transform.SetParent(rootCanvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (representedItem == null)
            return;

        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (representedItem == null)
        {
            SnapBack();
            return;
        }

        canvasGroup.blocksRaycasts = true;


        bool isDropped = TryDropIntoPlayerGrid(eventData.position) || TryDropIntoHubGrid(eventData.position);

        if (!isDropped)
        {
            SnapBack();
        }

        InventoryManager.instance.SaveInventoryState();
    }

    private bool TryDropIntoPlayerGrid(Vector2 screenPosition)
    {
        GameObject playerGridObject = InventoryManager.instance.GetPlayerGrid();
        if (playerGridObject == null)
            return false;

        RectTransform gridRect = playerGridObject.GetComponent<RectTransform>();
        if (gridRect == null)
            return false;

        if (!RectTransformUtility.RectangleContainsScreenPoint(gridRect, screenPosition, null))
            return false;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Vector2 topLeftScreenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[1]);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRect, topLeftScreenPoint, null, out Vector2 localPoint);

        float slotWidth = InventoryManager.instance.GetPlayerGridSlotWidth();
        float slotHeight = InventoryManager.instance.GetPlayerGridSlotHeight();

        float rectWidth = gridRect.rect.width;
        float rectHeight = gridRect.rect.height;

        float xFromLeft = localPoint.x + (rectWidth * 0.5f);
        float yFromTop = (rectHeight * 0.5f) - localPoint.y;

        int targetX = Mathf.FloorToInt(xFromLeft / slotWidth);
        int targetY = Mathf.FloorToInt(yFromTop / slotHeight);

        InGameItem wi = representedItem.GetComponent<InGameItem>();
        if (wi == null || wi.itemInstance == null)
            return false;

        int oldX = wi.itemInstance.x;
        int oldY = wi.itemInstance.y;

        InventoryManager.instance.playerGridReal.RemoveItem(representedItem);

        if (InventoryManager.instance.playerGridReal.CanPlaceWeaponAt(representedItem, targetX, targetY))
        {
            bool placed = InventoryManager.instance.playerGridReal.PlaceWeaponAt(representedItem, targetX, targetY);

            if (placed)
            {
                transform.SetParent(InventoryManager.instance.GetPlayerItemLayer(), false);

                float posX = wi.itemInstance.x * slotWidth;
                float posY = -(wi.itemInstance.y * slotHeight);

                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                rectTransform.anchoredPosition = new Vector2(posX, posY);

                return true;
            }
        }

        InventoryManager.instance.playerGridReal.PlaceWeaponAt(representedItem, oldX, oldY);
        return false;
    }

    private bool TryDropIntoHubGrid(Vector2 screenPosition)
    {
        GameObject hubMenu = InventoryManager.instance.GetHubInventoryMenu();

        if (hubMenu == null || !hubMenu.activeInHierarchy)
        {
            return false;
        }

        GameObject gridObject = InventoryManager.instance.GetHubGrid();
        if (gridObject == null)
            return false;

        RectTransform gridRect = gridObject.GetComponent<RectTransform>();
        if (gridRect == null)
            return false;

        if (!RectTransformUtility.RectangleContainsScreenPoint(gridRect, screenPosition, null)) 
            return false;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Vector2 topLeftScreenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[1]);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRect, topLeftScreenPoint, null, out Vector2 localPoint);

        float slotWidth = InventoryManager.instance.playerGridSlotWidth;
        float slotHeight = InventoryManager.instance.playerGridSlotHeight;

        float xFromLeft = localPoint.x + (gridRect.rect.width * 0.5f);
        float yFromTop = (gridRect.rect.height * 0.5f) - localPoint.y;

        int targetX = Mathf.FloorToInt(xFromLeft / slotWidth);
        int targetY = Mathf.FloorToInt(yFromTop / slotHeight);

        InGameItem wi = representedItem.GetComponent<InGameItem>();
        int oldX = wi.itemInstance.x;
        int oldY = wi.itemInstance.y;


        bool cameFromHub = originalParent == InventoryManager.instance.GetHubItemLayer();
        InventoryManager.instance.playerGridReal.RemoveItem(representedItem);
        InventoryManager.instance.hubGridReal.RemoveItem(representedItem);

        if (InventoryManager.instance.hubGridReal.CanPlaceWeaponAt(representedItem, targetX, targetY))
        {
            if (InventoryManager.instance.hubGridReal.PlaceWeaponAt(representedItem, targetX, targetY))
            {
                transform.SetParent(InventoryManager.instance.GetHubItemLayer(), false);

                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);

                rectTransform.anchoredPosition = new Vector2(wi.itemInstance.x * slotWidth, -(wi.itemInstance.y * slotHeight));



                InventoryManager.instance.plrInventoryReference.playerInventory.Remove(representedItem);
                InventoryManager.instance.hubInventoryReference.hubInventory.Add(representedItem);

                return true;
            }
        }

        if (cameFromHub)
        {
            InventoryManager.instance.hubGridReal.PlaceWeaponAt(representedItem, oldX, oldY);
        }
        else
        { 

            InventoryManager.instance.playerGridReal.PlaceWeaponAt(representedItem, oldX, oldY);
        }

        return false;
    }

    private void SnapBack()
    {
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }
}
