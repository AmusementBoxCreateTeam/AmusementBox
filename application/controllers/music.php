<?php

if (!defined('BASEPATH'))
    exit('No direct script access allowed');

class Music extends CI_Controller {

    public function __construct() {
        parent::__construct();
        $this->output->enable_profiler(TRUE);
        $this->config->load('user');
        $this->load->model(array('musics'));
        $this->load->library(array('pagination'));
    }

    public function index() {
        $search = '';
        $data['per_page'] = '';
        if (!empty($_GET)) {
            $search = $this->input->get();
            $data['search'] = $search;
            $data['per_page'] = !empty($search['per_page']) ? $search['per_page'] : '';
        }

        $data['list'] = $this->musics->get_list($search, $data['per_page']);
        
        //ページング
        //件数を取得
        $data['count_row'] = $this->musics->get_count_row($search);
        $index = 0;
        $query = '';
        if (!empty($search)) {
            foreach ($search as $k => $v) {
                if (is_array($v)) {
                    foreach ($v as $v2) {
                        $query .= '&' . ($k . '[]') . '=' . ($v2);
                    }
                } else {
                    $query .= '&' . ($k) . '=' . ($v);
                }
                $index++;
            }
            $query = substr_replace($query, '?', 0, 1);
        }
        $url = base_url() . 'index.php/user';
        if (!empty($query)) {
            $url .= $query;
        }
        $data['paging'] = '';
        $data['paging'] = $this->pagination($url, $data['count_row']);
        $data['start_row'] = ($data['count_row'] === 0) ? 0 : $data['per_page'] + 1;
        $data['end_row'] = ($data['count_row'] === 0 || $data['count_row'] <= $this->config->item('disp_num')) ? 0 : $data['per_page'] + $this->config->item('disp_num');

        $this->load->view('music/index',$data);
    }
    
    
    /**
     * ページング設定
     * @param type $url
     * @param type $total_rows
     */
    private function pagination($url = '', $total_rows = '') {

        $config['first_link'] = '最初';
        $config['last_link'] = '最後';
        $config['page_query_string'] = TRUE;
        $config['base_url'] = $url;
        $config['total_rows'] = $total_rows;
        $config['per_page'] = $this->config->item('disp_num');

        $this->pagination->initialize($config);

        return $this->pagination->create_links();
    }

}
