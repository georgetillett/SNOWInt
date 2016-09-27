function selectSprint() {
    var url = "./SelectSprint.aspx";
    var parameter = " dialogWidth=50px; dialogHeight=100px; center=yes; help=yes; status=no; resizeable=no;";

    var returnParameter = window.showModalDialog(url, parameter);

    if (returnParameter != null) {
        var querystringValues = getUrlVars();
        var projectGroupValue = querystringValues["ProjectGroup"];
        var sprintValue = returnParameter;

        var newQueryString = "?";
        newQueryString += addQueryStringValue(newQueryString, "ProjectGroup", projectGroupValue);
        newQueryString += addQueryStringValue(newQueryString, "Sprint", sprintValue);

        window.location.href = getBaseUrl() + newQueryString;
    }
}

function getBaseUrl() {
    return document.URL.split("?")[0];
}

function addQueryStringValue(querystring, name, value) {
    if (value == null) return "";

    var newParam = "";
    if (querystring.length > 1) {
        newParam += "&";
    }
    newParam += name + "=" + value;

    return newParam;
}

function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
		        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}
