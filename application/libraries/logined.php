<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Logined {

    public function __construct() {
        $CI = & get_instance();
        $CI->load->library('session');
    }

    public function logincheck() {
        $CI = & get_instance();
        $user_id = $CI->session->userdata('login_id');
        if (empty($user_id)) {
            redirect('index/login');
        }
    }

}
