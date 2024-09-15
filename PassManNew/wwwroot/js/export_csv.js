function exportToCsv(filename, rows) {

    var processHeading = function (row) {
        var finalVal = '';

        for (var j = 0; j < Object.keys(row).length; j++) {

            var innerValue = Object.keys(row)[j] === null ? '' : Object.keys(row)[j].toString();

            var result = innerValue.replace(/"/g, '""');
            if (result.search(/("|,|\n)/g) >= 0)
                result = '"' + result + '"';
            if (j > 0)
                finalVal += ',';
            finalVal += result;
        }

        return finalVal + '\n';
    };

    var processRow = function (row) {
        var finalVal = '';

        for (var j = 0; j < Object.keys(row).length; j++) {

            var innerValue = Object.values(row)[j] === null ? '' : Object.values(row)[j].toString();
            
            if (Object.keys(row)[j] instanceof Date) {
                innerValue = Object.values(row)[j].toLocaleString();
            };

            var result = innerValue.replace(/"/g, '""');
            if (result.search(/("|,|\n)/g) >= 0)
                result = '"' + result + '"';
            if (j > 0)
                finalVal += ',';
            finalVal += result;
        }

        return finalVal + '\n';
    };

    var csvFile = '';
    csvFile += processHeading(rows[0]);


    for (var i = 0; i < rows.length; i++) {
        csvFile += processRow(rows[i]);
    }
    var blob = new Blob(['\ufeff' + csvFile], { type: 'text/csv;charset=utf-8;' });
    if (navigator.msSaveBlob) { // IE 10+
        navigator.msSaveBlob(blob, filename);
    } else {
        var link = document.createElement("a");
        if (link.download !== undefined) { // feature detection
            // Browsers that support HTML5 download attribute
            var url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", filename);
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    }
}