
var myJsonResult;

function RunMacro() {
    // Embedded UI.Vision RPA Macros V3.61, License: MIT License (Open-Source), (c) 2020 a9t9
    //This code snippet checks if UI.Vision RPA is installed, and if yes, tells the RPA software to import and run the macro.
    //In UI.Vision RPA itself the user can allow/not allow to run embedded web macros (OFF by default)
    //To run web macros from specific websites without warning prompt, they URLs of certain websites can be whitelisted
    //For more details please see https://ui.vision/docs#embed
    (function (detail) {        
        var isExtensionLoaded = function () {
            var $root = document.documentElement
            return !!$root && !!$root.getAttribute('data-kantu')
        }
        var openExternal = function (url) {
            const $el = document.createElement('a')
            $el.setAttribute('target', '_blank')
            $el.setAttribute('href', url)
            $el.style.position = 'absolute'
            $el.style.top = '-9999px'
            $el.style.left = '-9999px'
            document.body.appendChild($el)
            $el.click()
            setTimeout(() => {
                $el.remove()
            }, 200)
        }
        var openWebsite = function () {
            openExternal('https://ui.vision/rpa/home/getrpa')
        }
        if (!isExtensionLoaded()) {
            if (confirm('UI.Vision RPA is not installed yet. Do you want to download it now?')) {
                return openWebsite()
            }
        } else {
            return window.dispatchEvent(new CustomEvent('kantuSaveAndRunMacro', { detail: detail }))
        }
    })
        ({
            direct: 1, //If the website URL is whitelisted, run the macro without prompt
            closeRPA: 0, //keep the UI.Vision RPA UI open after the macro has completed
            continueInLastUsedTab: 0,
            json: myJsonResult 
        })
}


function RunRPA(linkId) {   
   
    $.ajax(
        {
            type: 'POST',
            dataType: 'JSON',
            url: '/Home/GetRPAJson',
            data: { linkId: linkId },            
            success:
                function (response) {
                    myJsonResult = JSON.stringify(response);
                    RunMacro();                    
                },
            error:
                function (response) {
                    alert("Error: " + response);
                }
        });

}