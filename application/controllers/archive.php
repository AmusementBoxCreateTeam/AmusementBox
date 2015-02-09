<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Archive extends CI_Controller {

    public function __construct() {
        parent::__construct();
        $this->output->enable_profiler(TRUE);
        $this->load->model('logins');
        $this->load->library('logined');
    }

    public function index(){
        $this->load->view('archive/select');
    }

    public function archive_site(){
        $this->load->view('archive/index');
    }
    
    public function onesheet(){
        $this->load->view('archive/onesheet');
    }
    
    public function impressions(){
        $this->load->view('archive/');
    }    
}
