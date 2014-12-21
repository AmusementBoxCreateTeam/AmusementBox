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
    if ($('#check_mail:checked').val() !== '1') {
        $('.account_mail').hide();
    }
    if ($('#check_pass:checked').val() !== '1') {
        $('.account_pass').hide();
    }
    $('#check_mail').click(function() {
        if ($('#check_mail:checked').val() !== '1') {
            $('.account_mail').hide();
        } else {
            $('.account_mail').show();
        }
    });
    $('#check_pass').click(function() {
        if ($('#check_pass:checked').val() !== '1') {
            $('.account_pass').hide();
        } else {
            $('.account_pass').show();
        }
    });
});