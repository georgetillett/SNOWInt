function showModalFilterPopup() {

    var popupTitle = "Update Filter";

    // Display the modal popup
    $("#ModalFilterDialog").dialog({
        title: popupTitle,
        dialogClass: "projectGroup",
        width: 700,
        modal: true,
        buttons: {
            Update: function () {

                updateWorkItemFilterValues();

                $(this).dialog('close');
            },
            Cancel: function () {
                $(this).dialog('close');
            }
        }
    });
}

function updateWorkItemFilterValues() {

    // Get the Status Values
    var storyStatusValues = GetCheckBoxListValues("ModalFilterStoryStatusCheckBoxList");
    var incidentStatusValues = GetCheckBoxListValues("ModalFilterIncidentStatusCheckBoxList");
    var assignedToDropdown = document.getElementById("ModalFilterAssingedToDropdown");
    //var assignedToDropdown = $("#ModalFilterAssingedToDropdown");

    var assignedToValue = assignedToDropdown.options[assignedToDropdown.selectedIndex].value;

    $.ajax({
        type: 'POST',
        url: 'WorkList.aspx/UpdateWorkItemFilter',
        data: JSON.stringify({
            assingedToGUID: assignedToValue,
            storyStatusValues: storyStatusValues,
            incidentStatusValues: incidentStatusValues
        }),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (msg) {

            // Refresh the page by simulating the UpdateFilterButton click event
            __doPostBack('UpdateFilterButton','')

        },
        error: function (msg) {
            // Notice that msg.d is used to retrieve the result object
            alert(msg);
        }
    });
}

function GetCheckBoxListValues(checkBoxListID) {
    var values = new Array();

    for (var index = 0; index < 20; index++) {
        var controlID = checkBoxListID + "_" + index;
        var checkBox = document.getElementById(controlID);

        if (checkBox != null && checkBox.checked) {
            values.push(checkBox.value);
        }
    }

    return values;
}

/*
<table id="ModalFilterStoryStatusCheckBoxList">
	<tr>
		<td><input id="ModalFilterStoryStatusCheckBoxList_0" type="checkbox" name="ModalFilterStoryStatusCheckBoxList$0" value="0" /><label for="ModalFilterStoryStatusCheckBoxList_0">Draft</label></td>
	</tr><tr>
		<td><input id="ModalFilterStoryStatusCheckBoxList_1" type="checkbox" name="ModalFilterStoryStatusCheckBoxList$1" value="1" /><label for="ModalFilterStoryStatusCheckBoxList_1">Awaiting Approval</label></td>
	</tr>
</table>

*/