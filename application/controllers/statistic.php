<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Statistic extends CI_Controller {

    public function __construct() {
        parent::__construct();
        $this->output->enable_profiler(TRUE);
        $this->load->library(array('session'));
        $this->load->model(array('statistics'));
    }

    public function index() {
        $data['list'] = $this->statistics->get_rank();
        $this->load->view('statistic/index.php', $data);
    }

    public function rank() {
        if (empty($_POST)) {
            show_404();
            exit();
        }
        $this->configuration();
        if (!$this->form_validation->run()) {
            $data['not_found'] = 'お探しの情報は見つかりませんでした';
        } else {
            $data['list'] = $this->statistics->get_rank($this->input->post());
        }
        $this->load->view('statistic/index.php', $data);
    }

    public function rank_detail($id = '') {
        if (empty($id)) {
            show_404();
            exit();
        }
        //音楽詳細
        $data['detail'] = $this->statistics->rank_graph($id,'','');
        //女性
        $data['woman'] = $this->statistics->rank_graph($id,0,'');
        //男性
        $data['man'] = $this->statistics->rank_graph($id,1,'');
        
        for($i=10; $i<100; $i = $i+10){
           $data['a'.$i] =  $this->statistics->rank_graph($id,'',$i); 
        }
        $this->load->view('statistic/detail',$data);
    }

    private function configuration() {
        $this->form_validation->set_rules('gender', '性別', 'trim');
        $this->form_validation->set_rules('age', '年代', 'trim');
    }

}
