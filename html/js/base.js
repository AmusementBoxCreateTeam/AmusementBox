$(function() {
    $('#datepicker1').datepicker({
        format: 'yyyy-mm-dd',
        language: 'ja',
        autoclose: true,
        clearBtn: true
    });
    if (!$('#datepicker1').val() === '') {
        $('#datepicker2').datepicker({
            format: 'yyyy-mm-dd',
            language: 'ja',
            autoclose: true,
            clearBtn: true
        });
    } else {
        $('#datepicker2').datepicker({
            format: 'yyyy-mm-dd',
            language: 'ja',
            autoclose: true,
            clearBtn: true,
            startDate: $('datepicker1').val()
        });
    }
    
});