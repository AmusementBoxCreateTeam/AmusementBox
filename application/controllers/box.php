<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Box extends CI_Controller {

    public function __construct() {
        parent::__construct();
        $this->output->enable_profiler(TRUE);
        $this->load->model(array('boxes'));
    }

    public function index() {
        $data['list'] = $this->boxes->get_list();
        $this->load->view('box/index.php', $data);
    }

}
